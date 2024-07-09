using PeterO.Cbor;
using WalletFramework.Functional;
using static WalletFramework.Mdoc.Common.Constants;
using static WalletFramework.Mdoc.ProtectedHeaders;
using static WalletFramework.Mdoc.UnprotectedHeaders;
using static WalletFramework.Mdoc.MobileSecurityObject;
using static WalletFramework.Mdoc.CoseSignature;
using static WalletFramework.Functional.ValidationFun;

namespace WalletFramework.Mdoc;

public readonly struct IssuerAuth
{
    public ProtectedHeaders ProtectedHeaders { get; }

    public UnprotectedHeaders UnprotectedHeaders { get; }

    public MobileSecurityObject Payload { get; }

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

    internal static Validation<IssuerAuth> ValidIssuerAuth(CBORObject issuerSigned) =>
        issuerSigned.GetByLabel(IssuerAuthLabel).OnSuccess(issuerAuth => 
                Valid(Create)
                    .Apply(ValidProtectedHeaders(issuerAuth))
                    .Apply(ValidUnprotectedHeaders(issuerAuth))
                    .Apply(ValidMobileSecurityObject(issuerAuth))
                    .Apply(ValidCoseSignature(issuerAuth))
        );

    public CBORObject Encode()
    {
        var cbor = CBORObject.NewArray();
        cbor.Add(ProtectedHeaders.ByteString);
        cbor.Add(UnprotectedHeaders.Encode());
        cbor.Add(Payload.ByteString);
        cbor.Add(CBORObject.FromObject(Signature.Value));

        return cbor;
    }
}
