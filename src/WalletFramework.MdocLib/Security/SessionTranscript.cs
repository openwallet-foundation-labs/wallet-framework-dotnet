using PeterO.Cbor;
using WalletFramework.MdocLib.Security.Abstractions;

namespace WalletFramework.MdocLib.Security;

public record SessionTranscript(IHandover Handover);

public static class SessionTranscriptFun
{
    public static CBORObject ToCbor(this SessionTranscript sessionTranscript)
    {
        var result = CBORObject.NewArray();

        result.Add(CBORObject.Null);
        result.Add(CBORObject.Null);
        result.Add(sessionTranscript.Handover.ToCbor());

        return result;
    }

    public static SessionTranscript ToSessionTranscript(this IHandover handover) => new(handover);
}
