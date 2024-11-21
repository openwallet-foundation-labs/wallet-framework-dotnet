using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Parameters;
using WalletFramework.MdocLib.Device.Request;
using WalletFramework.MdocLib.Reader;
using WalletFramework.MdocLib.Security;

namespace WalletFramework.IsoProximity.CommunicationPhase.Abstractions;

public interface IProximityCommunicationService
{
    Task<(DeviceRequest, SessionTranscript, ECPrivateKeyParameters)> HandleReaderEngagement(ReaderEngagement readerEngagement);
}
