using PeterO.Cbor;

namespace WalletFramework.MdocLib.Device;

public record DeviceSigned(DeviceNameSpaces DeviceNameSpaces, DeviceAuth DeviceAuth);

public static class DeviceSignedFun
{
    public static CBORObject ToCbor(this DeviceSigned deviceSigned)
    {
        var cbor = CBORObject.NewMap();

        var a = deviceSigned.DeviceNameSpaces;
        var b = deviceSigned.DeviceNameSpaces.ToCborByteString();
        CBORObject c = b;
        
        cbor.Add("nameSpaces", c);
        cbor.Add("deviceAuth", deviceSigned.DeviceAuth.ToCbor());

        return cbor;
    }
}
