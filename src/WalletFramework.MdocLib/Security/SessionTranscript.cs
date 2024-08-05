using PeterO.Cbor;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Security.Abstractions;

namespace WalletFramework.MdocLib.Security;

public record SessionTranscript(IHandover Handover);

public static class SessionTranscriptFun
{
    public static CborByteString ToCborByteString(this SessionTranscript sessionTranscript)
    {
        var result = CBORObject.NewArray();

        result.Add(CBORObject.Null);
        result.Add(CBORObject.Null);

        var handOver = sessionTranscript.Handover.ToCbor();
        result.Add(handOver);

        // TODO: Fix this
        var x = result.ToCborByteString();
        return x;
    }

    public static SessionTranscript ToSessionTranscript(this IHandover handover) => new(handover);
}
