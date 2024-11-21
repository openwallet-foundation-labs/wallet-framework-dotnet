using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Security.Cose;

namespace WalletFramework.MdocLib.Device;

public record DeviceSignature(ProtectedHeaders ProtectedHeaders, CoseSignature Signature)
{
    public static Validation<DeviceSignature> FromCbor(CBORObject cbor)
    {
        var headersValidation = 
            from headers in ProtectedHeaders.ValidProtectedHeaders(cbor)
            select headers;

        var signatureValidation =
            from signature in CoseSignature.ValidCoseSignature(cbor)
            select signature;

        return
            from protectedHeaders in headersValidation
            from signature in signatureValidation
            select new DeviceSignature(protectedHeaders, signature);
    }
}

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
