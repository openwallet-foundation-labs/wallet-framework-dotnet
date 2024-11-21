using PeterO.Cbor;
using WalletFramework.MdocLib.Device;
using WalletFramework.MdocLib.Security.Cose;

namespace WalletFramework.MdocLib.Security;

// Currently only needed for DeviceAuth
// public record SigStructure(DeviceAuthentication Payload, ProtectedHeaders ProtectedHeaders);
public record SigStructure(CBORObject Payload, ProtectedHeaders ProtectedHeaders);

public static class SigStructureFun
{
    public static CBORObject ToCbor(this SigStructure sigStructure)
    {
        var result = CBORObject.NewArray();

        var context = CBORObject.FromObject("Signature1");
        result.Add(context);

        var protectedHeaders = sigStructure.ProtectedHeaders.AsCborByteString;
        result.Add(protectedHeaders);

        var externalAdd = CBORObject.FromObject(Array.Empty<byte>());
        result.Add(externalAdd);

        var payload = sigStructure.Payload;
        result.Add(payload);

        return result;
    }
}
