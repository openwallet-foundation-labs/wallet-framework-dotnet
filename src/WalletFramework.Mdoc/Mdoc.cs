using System.Security.Cryptography;
using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using PeterO.Cbor;
using WalletFramework.Functional;
using WalletFramework.Mdoc.Common;
using static WalletFramework.Mdoc.DocType;
using static WalletFramework.Mdoc.IssuerSigned;
using static WalletFramework.Functional.ValidationFun;
using static WalletFramework.Mdoc.Common.Constants;

namespace WalletFramework.Mdoc;

public readonly struct Mdoc
{
    public DocType DocType { get; }

    public IssuerSigned IssuerSigned { get; init; }

    // TODO: mdoc authentication
    // public DeviceSigned DeviceSigned { get; }

    private Mdoc(
        DocType docType,
        IssuerSigned issuerSigned)
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

        var validCbor =
            from bytes in decodeBase64Url(base64UrlencodedCborByteString)
            from cborObject in parseCborByteString(bytes)
            select cborObject;

        return 
            from cbor in validCbor
            from mdoc in Valid(Create)
                .Apply(ValidDoctype(cbor))
                .Apply(ValidIssuerSigned(cbor))
            from validMdoc in validateIntegrity(mdoc)
            select validMdoc;
    }

    public record InvalidBase64UrlEncodingError(Exception E) : Error("String is not Base64UrlEncoded", E);
}

public static class MdocFun
{
    // TODO: Implement this, if needed
    // public static Validation<Mdoc> ValidateCertificate()
    // {
    // }

    public static string Encode(this Mdoc mdoc)
    {
        var cbor = CBORObject.NewMap();
        
        cbor[DocTypeLabel] = mdoc.DocType.Encode();
        cbor[IssuerSignedLabel] = mdoc.IssuerSigned.Encode();

        var bytes = cbor.EncodeToBytes();
        return Base64UrlEncoder.Encode(bytes);
    }

    public static Validation<Mdoc> DigestsMatch(this Mdoc mdoc)
    {
        var potentialErrors = mdoc
            .IssuerSigned
            .NameSpaces
            .Value
            .SelectMany(pair => mdoc.IssuerSigned.NameSpaces[pair.Key].Select(item => (pair.Key, item)))
            .Select(nameSpaceAndItem =>
            {
                var nameSpace = nameSpaceAndItem.Key;
                var issuerSignedItem = nameSpaceAndItem.item;
                    
                byte[] digest = mdoc.IssuerSigned.IssuerAuth.Payload.ValueDigests[nameSpace][issuerSignedItem.DigestId];
                byte[] bytes = issuerSignedItem.ByteString.EncodedBytes;
                
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
        
        if (mdocDocType.Value == msoDocType.Value)
        {
            return mdoc;
        }
        else
        {
            return new DocTypeInMdocDoesNotMatchWithDocTypeInMsoError(mdocDocType, msoDocType);
        }
    }

    public static Unit SelectivelyDisclose(
        this Mdoc mdoc,
        NameSpace nameSpace,
        IEnumerable<ElementIdentifier> elementsToDisclose)
    {
        var disclosures = mdoc
            .IssuerSigned
            .NameSpaces[nameSpace]
            .Filter(item => elementsToDisclose.Contains(item.ElementId))
            .ToList();

        var nameSpaces = mdoc.IssuerSigned.NameSpaces;
        nameSpaces.Value[nameSpace] = disclosures;

        return Unit.Default;
    }

    // TODO: Implement this, if needed
    // public static Validation<Mdoc> ValidateIssuerSignature()
    // {
    // }
}
