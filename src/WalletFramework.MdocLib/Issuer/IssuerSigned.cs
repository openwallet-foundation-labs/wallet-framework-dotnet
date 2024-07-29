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
    public IssuerNameSpaces IssuerNameSpaces { get; init; }

    public IssuerAuth IssuerAuth { get; }

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

    public CBORObject Encode()
    {
        var cbor = CBORObject.NewMap();
        
        cbor[NameSpacesLabel] = IssuerNameSpaces.ToCbor();
        cbor[IssuerAuthLabel] = IssuerAuth.Encode();

        return cbor;
    }
}
