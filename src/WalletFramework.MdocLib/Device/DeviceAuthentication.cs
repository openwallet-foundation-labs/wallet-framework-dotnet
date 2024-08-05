using PeterO.Cbor;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Security;

namespace WalletFramework.MdocLib.Device;

// TODO: This is used for calculation of the signature
public record DeviceAuthentication(
    SessionTranscript SessionTranscript,
    DocType DocType,
    DeviceNameSpaces DeviceNameSpaces);

public static class DeviceAuthenticationFun
{
    public static CborByteString ToCborByteString(this DeviceAuthentication deviceAuthentication)
    {
        var result = CBORObject.NewArray();

        var name = CBORObject.FromObject("DeviceAuthentication");
        result.Add(name);

        var sessionTranscript = deviceAuthentication.SessionTranscript.ToCborByteString().Decode();
        result.Add(sessionTranscript);

        var docType = CBORObject.FromObject(deviceAuthentication.DocType.ToString());
        result.Add(docType);

        var nameSpaces = deviceAuthentication.DeviceNameSpaces.ToCborByteString();
        result.Add(nameSpaces);

        return result.ToCborByteString();
    }
}
