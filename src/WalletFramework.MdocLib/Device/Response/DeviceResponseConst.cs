using PeterO.Cbor;

namespace WalletFramework.MdocLib.Device.Response;

public static class DeviceResponseConst
{
    public static CBORObject DocumentsLabel => CBORObject.FromObject("documents");

    public static CBORObject StatusLabel => CBORObject.FromObject("status");

    public static CBORObject VersionLabel => CBORObject.FromObject("version");
}
