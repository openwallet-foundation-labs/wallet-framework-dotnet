using PeterO.Cbor;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Device;

namespace WalletFramework.MdocLib.Security;

public record SigStructure(DeviceAuthentication Payload);

public static class SigStructureFun
{
    public static CborByteString ToCborByteString(this SigStructure sigStructure)
    {
        var result = CBORObject.NewArray();

        var context = CBORObject.FromObject("Signature1");
        result.Add(context);

        var externalAdd = CBORObject.FromObject(Array.Empty<byte>());
        result.Add(externalAdd);

        var payload = sigStructure.Payload.ToCborByteString();
        result.Add(payload);

        return result.ToCborByteString();
    }
}
