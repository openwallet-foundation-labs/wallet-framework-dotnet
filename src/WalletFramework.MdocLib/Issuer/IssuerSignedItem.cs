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

            var validDigestId =
                from digestIdCbor in issuerSignedItemDecoded.GetByLabel("digestID")
                from digestId in ValidDigestId(digestIdCbor)
                select digestId;

            var validRandom =
                from randomCbor in issuerSignedItemDecoded.GetByLabel("random")
                from random in ValidRandom(randomCbor)
                select random;
            
            var validElementIdentifier =
                from elementIdentifierCbor in issuerSignedItemDecoded.GetByLabel("elementIdentifier")
                from elementIdentifier in ValidElementIdentifier(elementIdentifierCbor)
                select elementIdentifier;
            
            var validElement =
                from elementCbor in issuerSignedItemDecoded.GetByLabel("elementValue")
                from element in ValidElement(elementCbor)
                select element;

            return 
                Valid(Create)
                .Apply(byteString)
                .Apply(validDigestId)
                .Apply(validRandom)
                .Apply(validElementIdentifier)
                .Apply(validElement);
        });
}
