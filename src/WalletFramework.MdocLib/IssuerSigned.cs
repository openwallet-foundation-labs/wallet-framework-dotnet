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

    internal static Validation<IssuerSigned> ValidIssuerSigned(CBORObject mdoc) =>
        mdoc.GetByLabel(IssuerSignedLabel).OnSuccess(issuerSigned =>
            Valid(Create)
                .Apply(ValidNameSpaces(issuerSigned))
                .Apply(ValidIssuerAuth(issuerSigned))
            );

    public CBORObject Encode()
    {
        var cbor = CBORObject.NewMap();
        
        cbor[NameSpacesLabel] = NameSpaces.Encode();
        cbor[IssuerAuthLabel] = IssuerAuth.Encode();

        return cbor;
    }
}
