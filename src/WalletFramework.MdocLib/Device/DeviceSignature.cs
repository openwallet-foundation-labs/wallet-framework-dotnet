using PeterO.Cbor;
using WalletFramework.MdocLib.Security.Cose;

namespace WalletFramework.MdocLib.Device;

public record DeviceSignature(ProtectedHeaders ProtectedHeaders, CoseSignature Signature);

public static class DeviceSignatureFun
{
    public static CBORObject ToCbor(this DeviceSignature deviceSignature)
    {
        var result = CBORObject.NewArray();

        result.Add(deviceSignature.ProtectedHeaders.AsCborByteString);
        result.Add(CBORObject.NewMap());
        result.Add(CBORObject.Null);
        result.Add(deviceSignature.Signature.AsCbor);

        return result;
    }
}
