using System.Security.Cryptography;
using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Device.Response;
using WalletFramework.MdocLib.Digests;
using WalletFramework.MdocLib.Elements;
using WalletFramework.MdocLib.Issuer;
using static WalletFramework.MdocLib.DocType;
using static WalletFramework.MdocLib.Issuer.IssuerSigned;
using static WalletFramework.Core.Functional.ValidationFun;
using static WalletFramework.MdocLib.Constants;

namespace WalletFramework.MdocLib;

public record Mdoc
{
    public DocType DocType { get; }

    public IssuerSigned IssuerSigned { get; init; }

    public Mdoc(DocType docType, IssuerSigned issuerSigned)
    {
        DocType = docType;
        IssuerSigned = issuerSigned;
    }

    private static Mdoc Create(DocType docType, IssuerSigned issuerSigned) => new(docType, issuerSigned);

    public static Validation<Mdoc> ValidMdoc(string base64UrlencodedCborByteString)
    {
        var decodeBase64Url = new Func<string, Validation<byte[]>>(str =>
        {
            try
            {
                return Base64UrlEncoder.DecodeBytes(str)!;
            }
            catch (Exception e)
            {
                return new InvalidBase64UrlEncodingError(e);
            }
        });

        var parseCborByteString = new Func<byte[], Validation<CBORObject>>(bytes =>
        {
            try
            {
                return CBORObject.DecodeFromBytes(bytes);
            }
            catch (Exception e)
            {
                return new InvalidCborByteStringError("mdocResponse", e);
            }
        });
        
        // Expiration and signature
        // TODO: Validate Certificate, if needed
        // TODO: Verify signature of IssuerAuth using working_public_key, working_public_key_parameters and working_public_key_algorithm from certificate, if needed
        // TODO: Validate expiration and other in ValidityInfo, if needed
        var validateIntegrity = new List<Validator<Mdoc>>
        {
            MdocFun.DocTypeMatches,
            MdocFun.DigestsMatch
        }
        .AggregateValidators();

        var validRootLevelMDocOne =  
            from bytes in decodeBase64Url(base64UrlencodedCborByteString)
            from cbor in parseCborByteString(bytes)
            from issuerSigned in cbor.GetByLabel(IssuerSignedLabel)
            from mdoc in Valid(Create)
                .Apply(ValidDoctype(cbor))
                .Apply(ValidIssuerSigned(issuerSigned))
            from validMdoc in validateIntegrity(mdoc)
            select validMdoc;
        
        //TODO: extract and validate all the mDocs in documents
        var validDocumentsNestedMDoc = 
            from bytes in decodeBase64Url(base64UrlencodedCborByteString)
            from cbor in parseCborByteString(bytes)
            from documents in cbor.GetByLabel(DocumentsLabel)
            from issuerSigned in documents[0].GetByLabel(IssuerSignedLabel)
            from mdoc in Valid(Create)
                .Apply(ValidDoctype(documents[0]))
                .Apply(ValidIssuerSigned(issuerSigned))
            from validMdoc in validateIntegrity(mdoc)
            select validMdoc;
        
        return validRootLevelMDocOne.Match(
            rootLevelMDoc => rootLevelMDoc,
            _ => validDocumentsNestedMDoc
        );
    }
    
    //TODO: Workaround because PId Issuer only implemented issuer signed, Delete this overload when PID Issuer is fixed!!
    public static Validation<Mdoc> FromIssuerSigned(string base64UrlencodedCborByteString)
    {
        var decodeBase64Url = new Func<string, Validation<byte[]>>(str =>
        {
            try
            {
                return Base64UrlEncoder.DecodeBytes(str)!;
            }
            catch (Exception e)
            {
                return new InvalidBase64UrlEncodingError(e);
            }
        });

        var parseCborByteString = new Func<byte[], Validation<CBORObject>>(bytes =>
        {
            try
            {
                return CBORObject.DecodeFromBytes(bytes);
            }
            catch (Exception e)
            {
                return new InvalidCborByteStringError("mdocResponse", e);
            }
        });
        
        var validateIntegrity = new List<Validator<Mdoc>>
        {
            MdocFun.DocTypeMatches,
            MdocFun.DigestsMatch
        }.AggregateValidators();

        return 
            from bytes in decodeBase64Url(base64UrlencodedCborByteString)
            from cbor in parseCborByteString(bytes)
            from issuerSigned in ValidIssuerSigned(cbor)
            from validMdoc in validateIntegrity(issuerSigned.ToMdoc())
            select validMdoc;
    }

    public record InvalidBase64UrlEncodingError(Exception E) : Error("String is not Base64UrlEncoded", E);
}

public static class MdocFun
{
    // public static Validation<Mdoc> ValidateCertificate(this Mdoc mdoc)
    // {
    //     var trustChain = mdoc.IssuerSigned.IssuerAuth.UnprotectedHeaders.X5Chain;
    //
    // }

    public static string Encode(this Mdoc mdoc)
    {
        var cbor = CBORObject.NewMap();
        
        cbor[DocTypeLabel] = mdoc.DocType.ToCbor();
        cbor[IssuerSignedLabel] = mdoc.IssuerSigned.ToCbor();

        var bytes = cbor.EncodeToBytes();
        return Base64UrlEncoder.Encode(bytes);
    }

    public static Validation<Mdoc> DigestsMatch(this Mdoc mdoc)
    {
        var potentialErrors = mdoc
            .IssuerSigned
            .IssuerNameSpaces
            .Value
            .SelectMany(pair => mdoc.IssuerSigned.IssuerNameSpaces[pair.Key].Select(item => (pair.Key, item)))
            .Select(nameSpaceAndItem =>
            {
                var nameSpace = nameSpaceAndItem.Key;
                var issuerSignedItem = nameSpaceAndItem.item;
                    
                byte[] digest = mdoc.IssuerSigned.IssuerAuth.Payload.ValueDigests[nameSpace][issuerSignedItem.DigestId];
                byte[] bytes = issuerSignedItem.ByteString.AsCbor.EncodeToBytes();
                
                DigestAlgorithmValue algorithm = mdoc.IssuerSigned.IssuerAuth.Payload.DigestAlgorithm;
                HashAlgorithm hashAlgorithm = algorithm switch
                {
                    DigestAlgorithmValue.Sha256 => SHA256.Create(),
                    DigestAlgorithmValue.Sha384 => SHA384.Create(),
                    DigestAlgorithmValue.Sha512 => SHA512.Create(),
                    _ => throw new InvalidOperationException("Invalid digest algorithm")
                };
                
                using (hashAlgorithm)
                {
                    var hash = hashAlgorithm.ComputeHash(bytes);
                    if (!hash.SequenceEqual(digest))
                    {
                        return new InvalidDigestError(
                            issuerSignedItem.DigestId,
                            BitConverter.ToString(hash),
                            BitConverter.ToString(digest)
                        );
                    }
                }
                
                return Option<InvalidDigestError>.None;
            });

        var errors = new List<Error>();
        foreach (var error in potentialErrors)
        {
            error.Match(
                value => errors.Add(value),
                () => { }
            );
        }

        return errors.Any() 
            ? errors.ToSeq()
            : mdoc;
    }

    public static Validation<Mdoc> DocTypeMatches(this Mdoc mdoc)
    {
        var mdocDocType = mdoc.DocType;
        var msoDocType = mdoc.IssuerSigned.IssuerAuth.Payload.DocType;
        
        if (mdocDocType.ToString() == msoDocType.ToString())
        {
            return mdoc;
        }
        else
        {
            return new DocTypeInMdocDoesNotMatchWithDocTypeInMsoError(mdocDocType, msoDocType);
        }
    }
    
    // TODO: Unpure, this can throw an exception when the namespace is not found, also mutates the dictionary
    public static Mdoc SelectivelyDisclose(
        this Mdoc mdoc,
        Dictionary<NameSpace, List<ElementIdentifier>> elementsToDisclose)
    {
        var nameSpaces = mdoc.IssuerSigned.IssuerNameSpaces;
        foreach (var (nameSpace, elements) in elementsToDisclose)
        {
            var disclosures = nameSpaces[nameSpace]
                .Filter(item => elements.Contains(item.ElementId))
                .ToList();
            
            nameSpaces.Value[nameSpace] = disclosures;
        }

        return mdoc;
    }

    // public static Validation<Mdoc> ValidateIssuerSignature(this Mdoc mdoc)
    // {
    //     
    // }

    // public static Validation<Mdoc> Validate(this Mdoc mdoc)
    // {
    //     var validate = new List<Validator<Mdoc>>
    //     {
    //         DocTypeMatches,
    //         DigestsMatch,
    //         ValidateCertificate,
    //         ValidateIssuerSignature,
    //         ValidateValidityInfo,
    //         ValidateDeviceSigned
    //     }.AggregateValidators();
    //
    //     return validate(mdoc);
    // }

    // public static Validation<Mdoc> ValidateValidityInfo(this Mdoc mdoc)
    // {
    //     
    // }

    public static Mdoc ToMdoc(this Document document)
    {
        var issuerSigned = document.IssuerSigned;
        var docType = document.DocType;

        return new Mdoc(docType, issuerSigned);
    }
}
