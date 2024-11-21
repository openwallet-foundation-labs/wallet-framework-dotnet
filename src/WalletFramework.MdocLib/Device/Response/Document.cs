using System.Diagnostics;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using PeterO.Cbor;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.Core.X509;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Device.Response.Errors;
using WalletFramework.MdocLib.Issuer;
using WalletFramework.MdocLib.Security;
using WalletFramework.MdocLib.Security.Cose;
using static WalletFramework.MdocLib.Constants;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace WalletFramework.MdocLib.Device.Response;

public record Document
{
    public DeviceSigned DeviceSigned { get; }

    public DocType DocType { get; }

    public IssuerSigned IssuerSigned { get; }

    public Document(AuthenticatedMdoc authentication)
    {
        DeviceSigned = authentication.DeviceSigned;
        DocType = authentication.Mdoc.DocType;
        IssuerSigned = authentication.Mdoc.IssuerSigned;
    }
    
    public Document(DocType docType, IssuerSigned issuerSigned, DeviceSigned deviceSigned)
    {
        DocType = docType;
        IssuerSigned = issuerSigned;
        DeviceSigned = deviceSigned;
    }
}

public static class DocumentFun
{
    public static Validation<Document> FromCbor(CBORObject cbor)
    {
        var docTypeValidation =
            from docType in DocType.ValidDoctype(cbor)
            select docType;
        
        var issuerSignedValidation = 
            from issuerSignedCbor in cbor.GetByLabel(IssuerSignedLabel)
            from issuerSigned in IssuerSigned.ValidIssuerSigned(issuerSignedCbor)
            select issuerSigned;
        
        var deviceSignedValidation = 
            from deviceSignedCbor in cbor.GetByLabel(DeviceSignedLabel)
            from deviceSigned in DeviceSigned.FromCbor(deviceSignedCbor)
            select deviceSigned;

        return
            from docType in docTypeValidation
            from issuerSigned in issuerSignedValidation
            from deviceSigned in deviceSignedValidation
            select new Document(docType, issuerSigned, deviceSigned);
    }
    
    public static CBORObject ToCbor(this Document document)
    {
        var cbor = CBORObject.NewMap();

        cbor.Add(DocTypeLabel, document.DocType.ToCbor());
        cbor.Add(IssuerSignedLabel, document.IssuerSigned.ToCbor());
        cbor.Add(DeviceSignedLabel, document.DeviceSigned.ToCbor());

        return cbor;
    }

    public static Validation<Document> ValidateIssuerDataAuthentication(this Document document)
    {
        return
            from pubKey in document.ValidateCertificate()
            from result in document.ValidateIssuerSignature(pubKey)
            select result;
    }

    // public static Validation<Document> ValidateDeviceSignedData(this Document document)
    // {
    // }
    
    public static string ByteArrayToString(this byte[] ba)
    {
        return BitConverter.ToString(ba).Replace("-","");
    }
    
    private static byte[] ConvertRawToDerFormat(byte[] rawSignature)
    {
        if (rawSignature.Length != 64)
            throw new ArgumentException("Raw signature should be 64 bytes long", nameof(rawSignature));

        var r = new BigInteger(1, rawSignature.Take(32).ToArray());
        var rHex = r.ToByteArrayUnsigned().ByteArrayToString();
        Debug.WriteLine($"R is {rHex} at {DateTime.Now:H:mm:ss:fff}");
        var s = new BigInteger(1, rawSignature.Skip(32).ToArray());
        var shex = s.ToByteArrayUnsigned().ByteArrayToString();
        Debug.WriteLine($"S is {shex} at {DateTime.Now:H:mm:ss:fff}");

        var derSignature = new DerSequence(
            new DerInteger(r),
            new DerInteger(s)
        ).GetDerEncoded();

        return derSignature;
    }

    private static Validation<Document> ValidateIssuerSignature(
        this Document document,
        ECPublicKeyParameters pubKey)
    {
        var issuerAuth = document.IssuerSigned.IssuerAuth;
        var signatureBytes = issuerAuth.Signature.AsByteArray;

        var payload = new SigStructure(issuerAuth.Payload.ByteString, issuerAuth.ProtectedHeaders);
        var payloadBytes = payload.ToCbor().EncodeToBytes();
        
        var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
        signer.Init(false, pubKey);

        signer.BlockUpdate(payloadBytes, 0, payloadBytes.Length);

        var der = ConvertRawToDerFormat(signatureBytes);

        var isValid = signer.VerifySignature(der);
        if (isValid)
        {
            Debug.WriteLine($"Signature is valid at {DateTime.Now:H:mm:ss:fff}");
            return document;
        }

        return new InvalidIssuerSignatureError();
    }

    private static Validation<ECPublicKeyParameters> ValidateCertificate(this Document document)
    {
        var trustChain = document.IssuerSigned.IssuerAuth.UnprotectedHeaders.TrustChain;

        var certs = trustChain
            .Select(cert2 => cert2.ToBouncyCastleX509Certificate())
            .ToList();

        try
        {
            var isValid = certs.IsTrustChainValid();
            Debug.WriteLine($"TrustChainIsValid is {isValid} at {DateTime.Now:H:mm:ss:fff}");
            if (isValid is false)
            {
                return new TrustChainValidationFailedError();
            }
            
            var endCert = certs.First();
            
            var workingPublicKey = endCert.GetPublicKey();
            return (ECPublicKeyParameters)workingPublicKey;
        }
        catch (Exception e)
        {
            return new TrustChainValidationFailedError(e);
        }
    }
}
