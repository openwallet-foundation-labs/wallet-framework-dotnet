using PeterO.Cbor;
using WalletFramework.MdocLib.Device;
using WalletFramework.MdocLib.Reader;
using WalletFramework.MdocLib.Security;
using WalletFramework.MdocLib.Security.Abstractions;

namespace WalletFramework.IsoProximity.EngagementPhase;

public record QrReaderHandover(
    ReaderEngagement ReaderEngagement,
    DeviceEngagement DeviceEngagement) : IHandover
{
    public CBORObject ToCbor() => 
        CBORObject.FromObject(ReaderEngagement.ToSha256Hash());

    public SessionTranscript ToSessionTranscript() =>
        new(DeviceEngagement,
            ReaderEngagement.EngagementSecurity.SenderKey.Value,
            this);
}
