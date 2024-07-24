using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Digests;
using WalletFramework.MdocLib.Elements;
using static WalletFramework.MdocLib.Cbor.CborByteString;
using static WalletFramework.MdocLib.Digests.DigestId;
using static WalletFramework.Core.Functional.ValidationFun;
using static WalletFramework.MdocLib.Security.Random;
using static WalletFramework.MdocLib.Elements.ElementIdentifier;
using static WalletFramework.MdocLib.Elements.Element;
using Random = WalletFramework.MdocLib.Security.Random;

namespace WalletFramework.MdocLib.Issuer;

public record IssuerSignedItem
{
    public CborByteString ByteString { get; }

    public Random RandomValue { get; }

    public ElementIdentifier ElementId { get; }

    public Element Element { get; }

    public DigestId DigestId { get; }

    private IssuerSignedItem(
        CborByteString byteString,
        DigestId digestId,
        Random randomValue,
        ElementIdentifier elementId,
        Element element)
    {
        ByteString = byteString;
        DigestId = digestId;
        RandomValue = randomValue;
        ElementId = elementId;
        Element = element;
    }

    private static IssuerSignedItem Create(
        CborByteString byteString,
        DigestId digestId,
        Random randomValue,
        ElementIdentifier elementId,
        Element value) =>
        new(byteString, digestId, randomValue, elementId, value);

    internal static Validation<IssuerSignedItem> ValidIssuerSignedItem(CBORObject issuerSignedItem) =>
        ValidCborByteString(issuerSignedItem).OnSuccess(byteString =>
        {
            var issuerSignedItemDecoded = byteString.Decode();

            return 
                Valid(Create)
                .Apply(byteString)
                .Apply(issuerSignedItemDecoded.GetByLabel("digestID").OnSuccess(ValidDigestId))
                .Apply(issuerSignedItemDecoded.GetByLabel("random").OnSuccess(ValidRandom))
                .Apply(issuerSignedItemDecoded.GetByLabel("elementIdentifier").OnSuccess(ValidElementIdentifier))
                .Apply(issuerSignedItemDecoded.GetByLabel("elementValue").OnSuccess(ValidElement));
        });
}
