using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;

namespace WalletFramework.MdocLib.Device;

public record DeviceSigned(Option<DeviceNameSpaces> DeviceNameSpaces, DeviceAuth DeviceAuth)
{
    public static Validation<DeviceSigned> FromCbor(CBORObject cbor)
    {
        var nameSpacesValidation = 
            from nameSpacesCbor in cbor.GetByLabel("nameSpaces")
            from deviceNameSpaces in Device.DeviceNameSpaces.FromCbor(nameSpacesCbor)
            select deviceNameSpaces;
        
        var deviceAuthValidation = 
            from deviceAuthCbor in cbor.GetByLabel("deviceAuth")
            from deviceAuth in DeviceAuth.FromCbor(deviceAuthCbor)
            select deviceAuth;

        return
            from deviceAuth in deviceAuthValidation
            let nameSpaces = nameSpacesValidation.ToOption()
            select new DeviceSigned(nameSpaces, deviceAuth);
    }
}

public static class DeviceSignedFun
{
    public static CBORObject ToCbor(this DeviceSigned deviceSigned)
    {
        var cbor = CBORObject.NewMap();

        cbor.Add("nameSpaces", deviceSigned.DeviceNameSpaces.ToCborByteString());
        cbor.Add("deviceAuth", deviceSigned.DeviceAuth.ToCbor());

        return cbor;
    }

    public static DeviceSigned ToDeviceSigned(this DeviceSignature deviceSignature, Option<DeviceNameSpaces> deviceNameSpaces)
    {
        var deviceAuth = new DeviceAuth(deviceSignature);
        return new DeviceSigned(deviceNameSpaces, deviceAuth);
    }
}
