using LanguageExt;
using PeterO.Cbor;

namespace WalletFramework.MdocLib.Device;

public record DeviceSigned(Option<DeviceNameSpaces> DeviceNameSpaces, DeviceAuth DeviceAuth);

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
