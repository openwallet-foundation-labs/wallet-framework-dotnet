using PeterO.Cbor;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Issuer;

namespace WalletFramework.MdocLib.Device;

public record DeviceNameSpaces(Dictionary<NameSpace, List<DeviceSignedItem>> Value);

public static class DeviceNameSpacesFun
{
    public static DeviceNameSpaces ToDeviceNameSpaces(this IssuerNameSpaces issuerNameSpaces)
    {
        var result = new Dictionary<NameSpace, List<DeviceSignedItem>>();
        
        foreach (var (nameSpace, issuerSignedItems) in issuerNameSpaces.Value)
        {
            var deviceSignedItems = issuerSignedItems
                .Select(issuerSignedItem => new DeviceSignedItem(issuerSignedItem.ElementId, issuerSignedItem.Element))
                .ToList();
            
            result.Add(nameSpace, deviceSignedItems);
        }

        return new DeviceNameSpaces(result);
    }
    
    public static CborByteString ToCborByteString(this DeviceNameSpaces deviceNameSpaces)
    {
        var result = CBORObject.NewMap();

        foreach (var (nameSpace, deviceSignedItems) in deviceNameSpaces.Value)
        {
            var itemsCbor = CBORObject.NewMap();
            foreach (var (elementIdentifier, elementValue) in deviceSignedItems)
            {
                var value = CBORObject.FromObject(elementValue.ToString());
                itemsCbor.Add(elementIdentifier.Value, value);
            }

            result.Add(nameSpace.ToString(), itemsCbor);
        }

        return result.ToCborByteString();
    }
}
