using PeterO.Cbor;

namespace WalletFramework.MdocLib.Device;

public record DeviceAuth(DeviceSignature DeviceSignature);

public static class DeviceAuthFun
{
    public static CBORObject ToCbor(this DeviceAuth deviceAuth)
    {
        var cbor = CBORObject.NewMap();

        cbor.Add("deviceSignature", deviceAuth.DeviceSignature.ToCbor());

        return cbor;
    }
}
