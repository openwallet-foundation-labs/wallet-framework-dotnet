using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Elements;
using static WalletFramework.MdocLib.Elements.ElementIdentifier;
using static WalletFramework.MdocLib.Elements.Element;

namespace WalletFramework.MdocLib.Device;

public record DeviceSignedItem(ElementIdentifier DataElementIdentifier, Element DataElementValue)
{
    public static Validation<DeviceSignedItem> FromCbor(CBORObject cbor)
    {
        var validElementIdentifier =
            from decoded in CborByteString.ValidCborByteString(cbor)
            from elementIdentifierCbor in decoded.AsCbor.GetByLabel("elementIdentifier")
            from elementIdentifier in ValidElementIdentifier(elementIdentifierCbor)
            select elementIdentifier;
            
        var validElement =
            from decoded in CborByteString.ValidCborByteString(cbor)
            from elementCbor in decoded.AsCbor.GetByLabel("elementValue")
            from element in ValidElement(elementCbor)
            select element;

        return
            from elementIdentifier in validElementIdentifier
            from element in validElement
            select new DeviceSignedItem(elementIdentifier, element);
    }
}
