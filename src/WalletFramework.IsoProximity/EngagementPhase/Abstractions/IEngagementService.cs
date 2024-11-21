using System.Threading.Tasks;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.MdocLib.Device;
using WalletFramework.MdocLib.Reader;

namespace WalletFramework.IsoProximity.EngagementPhase.Abstractions;

public interface IEngagementService
{
    Task<DeviceEngagement> CreateDeviceEngagement(PublicKey mdocPubKey);

    Task<ReaderEngagement> CreateReaderEngagement(PublicKey verifierPubKey);
}
