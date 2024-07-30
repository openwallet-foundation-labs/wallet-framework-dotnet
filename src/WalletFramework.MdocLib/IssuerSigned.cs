using PeterO.Cbor;
using WalletFramework.Core.Functional;
using static WalletFramework.MdocLib.Common.Constants;
using static WalletFramework.MdocLib.NameSpaces;
using static WalletFramework.MdocLib.IssuerAuth;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.MdocLib;

public record IssuerSigned
{
    public NameSpaces NameSpaces { get; init; }

    public IssuerAuth IssuerAuth { get; }

    private IssuerSigned(NameSpaces nameSpaces, IssuerAuth issuerAuth)
    {
        NameSpaces = nameSpaces;
        IssuerAuth = issuerAuth;
    }

    private static IssuerSigned Create(NameSpaces nameSpaces, IssuerAuth issuerAuth) =>
        new(nameSpaces, issuerAuth);

    public static Validation<IssuerSigned> ValidIssuerSigned(CBORObject issuerSigned) => 
        Valid(Create)
            .Apply(ValidNameSpaces(issuerSigned))
            .Apply(ValidIssuerAuth(issuerSigned));

    public CBORObject Encode()
    {
        var cbor = CBORObject.NewMap();
        
        cbor[NameSpacesLabel] = NameSpaces.Encode();
        cbor[IssuerAuthLabel] = IssuerAuth.Encode();

        return cbor;
    }
}

public static class IssuerSignedFun
{
    // TODO: This is only a hack currently, the doctype of the mdoc and the mso must be validated normally
    public static Mdoc ToMdoc(this IssuerSigned issuerSigned)
    {
        var docType = issuerSigned.IssuerAuth.Payload.DocType;
        return new Mdoc(docType, issuerSigned);
    }
}
