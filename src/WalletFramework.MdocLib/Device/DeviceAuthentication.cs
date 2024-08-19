using LanguageExt;
using PeterO.Cbor;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Security;

namespace WalletFramework.MdocLib.Device;

public record DeviceAuthentication(
    SessionTranscript SessionTranscript,
    DocType DocType,
    Option<DeviceNameSpaces> DeviceNameSpaces);

public static class DeviceAuthenticationFun
{
    public static CBORObject ToCbor(this DeviceAuthentication deviceAuthentication)
    {
        var result = CBORObject.NewArray();

        var name = CBORObject.FromObject("DeviceAuthentication");
        result.Add(name);

        var sessionTranscript = deviceAuthentication.SessionTranscript.ToCbor();
        result.Add(sessionTranscript);

        var docType = deviceAuthentication.DocType.ToCbor();
        result.Add(docType);

        var nameSpaces = deviceAuthentication.DeviceNameSpaces.ToCborByteString();
        result.Add(nameSpaces);

        var taggedByteString = result.ToTaggedCborByteString();
        var byteString = taggedByteString.AsCbor.ToCborByteString();
        
        return byteString;
    }
}
