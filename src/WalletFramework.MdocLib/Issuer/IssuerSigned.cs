using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using static WalletFramework.MdocLib.Constants;
using static WalletFramework.MdocLib.Issuer.IssuerNameSpaces;
using static WalletFramework.MdocLib.Issuer.IssuerAuth;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.MdocLib.Issuer;

public record IssuerSigned
{
    public IssuerNameSpaces IssuerNameSpaces { get; }

    public IssuerAuth IssuerAuth { get; init; }

    private IssuerSigned(IssuerNameSpaces issuerNameSpaces, IssuerAuth issuerAuth)
    {
        IssuerNameSpaces = issuerNameSpaces;
        IssuerAuth = issuerAuth;
    }

    private static IssuerSigned Create(IssuerNameSpaces issuerNameSpaces, IssuerAuth issuerAuth) =>
        new(issuerNameSpaces, issuerAuth);

    internal static Validation<IssuerSigned> ValidIssuerSigned(CBORObject mdoc) =>
        mdoc.GetByLabel(IssuerSignedLabel).OnSuccess(issuerSigned =>
            Valid(Create)
                .Apply(ValidNameSpaces(issuerSigned))
                .Apply(ValidIssuerAuth(issuerSigned))
            );
}

public static class IssuerSignedFun
{
    public static CBORObject ToCbor(this IssuerSigned issuerSigned)
    {
        var cbor = CBORObject.NewMap();
        
        cbor[NameSpacesLabel] = issuerSigned.IssuerNameSpaces.ToCbor();
        cbor[IssuerAuthLabel] = issuerSigned.IssuerAuth.Encode();

        return cbor;
    }
}
