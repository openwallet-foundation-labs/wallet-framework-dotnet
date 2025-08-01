using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Device;
using WalletFramework.MdocLib.Security.Abstractions;
using WalletFramework.MdocLib.Security.Cose;

namespace WalletFramework.MdocLib.Security;

public record SessionTranscript(
    Option<DeviceEngagement> DeviceEngagement,
    Option<PublicKey> ReaderKey,
    Option<IHandover> Handover);

public static class SessionTranscriptFun
{
    // TODO: Extend with actual
    public static CBORObject ToCbor(this SessionTranscript sessionTranscript)
    {
        var result = CBORObject.NewArray();

        sessionTranscript.DeviceEngagement.Match(
            engagement =>
            {
                var deviceEngagementBytes = engagement.ToCbor().ToTaggedCborByteString();

                result.Add(deviceEngagementBytes.AsCbor);
            },
            () =>
            {
                result.Add(CBORObject.Null);
            }
        );

        sessionTranscript.ReaderKey.Match(
            key =>
            {
                var keyBytes = key.ToCoseKey().ToCbor().ToTaggedCborByteString();
        
                result.Add(keyBytes.AsCbor);
            },
            () =>
            {
                result.Add(CBORObject.Null);
            }
        );

        sessionTranscript.Handover.Match(
            handover => { result.Add(handover.ToCbor()); },
            () => { result.Add(CBORObject.Null); }
        );
        
        return result;
    }
}
