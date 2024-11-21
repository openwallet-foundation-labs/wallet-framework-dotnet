using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Device;
using WalletFramework.MdocLib.Security.Abstractions;
using WalletFramework.MdocLib.Security.Cose;

namespace WalletFramework.MdocLib.Security;

public record SessionTranscript(
    Option<DeviceEngagement> DeviceEngagement,
    Option<PublicKey> ReaderKey,
    IHandover Handover);

public static class SessionTranscriptFun
{
    // TODO: Extend with actual
    public static CBORObject ToCbor(this SessionTranscript sessionTranscript)
    {
        var result = CBORObject.NewArray();

        sessionTranscript.DeviceEngagement.OnSome(engagement =>
        {
            var deviceEngagementBytes = engagement.ToCbor().ToTaggedCborByteString();

            result.Add(deviceEngagementBytes.AsCbor);

            return Unit.Default;
        });

        sessionTranscript.ReaderKey.OnSome(key =>
        {
            var keyBytes = key.ToCoseKey().ToCbor().ToTaggedCborByteString();
            
            result.Add(keyBytes.AsCbor);
            
            return Unit.Default;
        });
        
        result.Add(sessionTranscript.Handover.ToCbor());

        return result;
    }
}
