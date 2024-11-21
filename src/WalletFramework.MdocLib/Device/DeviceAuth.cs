using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;

namespace WalletFramework.MdocLib.Device;

public record DeviceAuth(DeviceSignature DeviceSignature)
{
    public static Validation<DeviceAuth> FromCbor(CBORObject cbor)
    {
        var signatureValidation =
            from signatureCbor in cbor.GetByLabel("deviceSignature")
            from signature in DeviceSignature.FromCbor(signatureCbor)
            select signature;

        return
            from signature in signatureValidation
            select new DeviceAuth(signature);
    }
}

public static class DeviceAuthFun
{
    public static CBORObject ToCbor(this DeviceAuth deviceAuth)
    {
        var cbor = CBORObject.NewMap();

        cbor.Add("deviceSignature", deviceAuth.DeviceSignature.ToCbor());

        return cbor;
    }
}
