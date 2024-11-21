using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Security;
using static WalletFramework.MdocLib.NameSpace;

namespace WalletFramework.MdocLib.Device;

public record DeviceNameSpaces(Dictionary<NameSpace, List<DeviceSignedItem>> Value)
{
    public static Validation<DeviceNameSpaces> FromCbor(CBORObject cbor)
    {
        var validDict = cbor
            .ToDictionary(
                ValidNameSpace, 
                issuerSignedItems =>
                {
                    return issuerSignedItems
                        .Values
                        .Select(DeviceSignedItem.FromCbor)
                        .TraverseAll(item => item)
                        .Select(items => items.ToList());
                });
        
        return
            from dict in validDict
            select new DeviceNameSpaces(dict);
    }
}

public static class DeviceNameSpacesFun
{
    public static DeviceNameSpaces ToDeviceNameSpaces(this KeyAuthorizations keyAuthorizations)
    {
        var result = new Dictionary<NameSpace, List<DeviceSignedItem>>();
        
        // TODO: Build device namespaces based on key authorizations

        return new DeviceNameSpaces(result);
    }
    
    public static CBORObject ToCborByteString(
        this Option<DeviceNameSpaces> deviceNameSpaces) => deviceNameSpaces.Match(nameSpaces =>
        {
            var result = CBORObject.NewMap();

            foreach (var (nameSpace, deviceSignedItems) in nameSpaces.Value)
            {
                var itemsCbor = CBORObject.NewMap();
                foreach (var (elementIdentifier, elementValue) in deviceSignedItems)
                {
                    var value = CBORObject.FromObject(elementValue.ToString());
                    itemsCbor.Add(elementIdentifier.Value, value);
                }

                result.Add(nameSpace.ToString(), itemsCbor);
            }

            return result.ToTaggedCborByteString();
        },
        () => CBORObject.NewMap().ToTaggedCborByteString());
}
