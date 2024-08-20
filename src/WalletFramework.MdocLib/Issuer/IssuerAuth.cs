using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Security;
using WalletFramework.MdocLib.Security.Cose;
using static WalletFramework.MdocLib.Constants;
using static WalletFramework.MdocLib.Security.Cose.ProtectedHeaders;
using static WalletFramework.MdocLib.Security.Cose.UnprotectedHeaders;
using static WalletFramework.MdocLib.Security.MobileSecurityObject;
using static WalletFramework.MdocLib.Security.Cose.CoseSignature;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.MdocLib.Issuer;

public record IssuerAuth
{
    public ProtectedHeaders ProtectedHeaders { get; }

    public UnprotectedHeaders UnprotectedHeaders { get; }

    public MobileSecurityObject Payload { get; init; }

    public CoseSignature Signature { get; }

    private IssuerAuth(
        ProtectedHeaders protectedHeaders,
        UnprotectedHeaders unprotectedHeaders,
        MobileSecurityObject payload,
        CoseSignature signature)
    {
        ProtectedHeaders = protectedHeaders;
        UnprotectedHeaders = unprotectedHeaders;
        Payload = payload;
        Signature = signature;
    }

    private static IssuerAuth Create(
        ProtectedHeaders protectedHeaders,
        UnprotectedHeaders unprotectedHeaders,
        MobileSecurityObject payload,
        CoseSignature signature) =>
        new(protectedHeaders, unprotectedHeaders, payload, signature);

    internal static Validation<IssuerAuth> ValidIssuerAuth(CBORObject issuerSigned) => issuerSigned
        .GetByLabel(IssuerAuthLabel)
        .OnSuccess(issuerAuth => 
            Valid(Create)
                .Apply(ValidProtectedHeaders(issuerAuth))
                .Apply(ValidUnprotectedHeaders(issuerAuth))
                .Apply(ValidMobileSecurityObject(issuerAuth))
                .Apply(ValidCoseSignature(issuerAuth)));

    public CBORObject ToCbor()
    {
        var cbor = CBORObject.NewArray();
        
        cbor.Add(ProtectedHeaders.AsCborByteString);
        cbor.Add(UnprotectedHeaders.Encode());
        cbor.Add(Payload.ByteString);
        cbor.Add(CBORObject.FromObject(Signature.AsByteArray));

        return cbor;
    }
}
