using PeterO.Cbor;
using WalletFramework.MdocLib.Security.Cose;

namespace WalletFramework.MdocLib.Device;

public record DeviceSignature(ProtectedHeaders ProtectedHeaders, CoseSignature Signature);

public static class DeviceSignatureFun
{
    public static CBORObject ToCbor(this DeviceSignature deviceSignature)
    {
        var result = CBORObject.NewArray();

        // var a = deviceSignature.ProtectedHeaders.AsCborByteString.Decode();
        // var b = a.EncodeToBytes();
        // var c = CBORObject.FromObject(b);
        
        result.Add(deviceSignature.ProtectedHeaders.AsCborByteString);
        result.Add(CBORObject.Null);
        result.Add(CBORObject.Null);
        result.Add(deviceSignature.Signature.AsByteString);

        return result;
    }
}
