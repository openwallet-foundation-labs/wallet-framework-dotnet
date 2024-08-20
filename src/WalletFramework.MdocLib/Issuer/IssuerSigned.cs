using PeterO.Cbor;
using WalletFramework.Core.Functional;
using static WalletFramework.MdocLib.Constants;
using static WalletFramework.MdocLib.Issuer.IssuerNameSpaces;
using static WalletFramework.MdocLib.Issuer.IssuerAuth;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.MdocLib.Issuer;

public record IssuerSigned
{
    public IssuerNameSpaces IssuerNameSpaces { get; }

    public IssuerAuth IssuerAuth { get; }

    private IssuerSigned(IssuerNameSpaces issuerNameSpaces, IssuerAuth issuerAuth)
    {
        IssuerNameSpaces = issuerNameSpaces;
        IssuerAuth = issuerAuth;
    }

    private static IssuerSigned Create(IssuerNameSpaces issuerNameSpaces, IssuerAuth issuerAuth) =>
        new(issuerNameSpaces, issuerAuth);

    public static Validation<IssuerSigned> ValidIssuerSigned(CBORObject issuerSigned) => 
        Valid(Create)
            .Apply(ValidNameSpaces(issuerSigned))
            .Apply(ValidIssuerAuth(issuerSigned));
}

public static class IssuerSignedFun
{
    public static CBORObject ToCbor(this IssuerSigned issuerSigned)
    {
        var cbor = CBORObject.NewMap();

        cbor[NameSpacesLabel] = issuerSigned.IssuerNameSpaces.ToCbor();
        cbor[IssuerAuthLabel] = issuerSigned.IssuerAuth.ToCbor();

        return cbor;
    }

    // TODO: This is only a hack currently, the doctype of the mdoc and the mso must be validated normally
    public static Mdoc ToMdoc(this IssuerSigned issuerSigned)
    {
        var docType = issuerSigned.IssuerAuth.Payload.DocType;
        return new Mdoc(docType, issuerSigned);
    }
}
