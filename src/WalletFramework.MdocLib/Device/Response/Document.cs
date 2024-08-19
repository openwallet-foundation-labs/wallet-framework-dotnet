using PeterO.Cbor;
using WalletFramework.MdocLib.Issuer;

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
}

public static class DocumentFun
{
    public static CBORObject ToCbor(this Document document)
    {
        var cbor = CBORObject.NewMap();

        cbor.Add(Constants.DocTypeLabel, document.DocType.ToCbor());
        cbor.Add(Constants.IssuerSignedLabel, document.IssuerSigned.ToCbor());
        cbor.Add("deviceSigned", document.DeviceSigned.ToCbor());

        return cbor;
    }
}
