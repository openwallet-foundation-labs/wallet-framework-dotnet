using System.Threading.Tasks;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.IsoProximity.EngagementPhase.Abstractions;
using WalletFramework.MdocLib.Device;
using WalletFramework.MdocLib.Device.Abstractions;
using WalletFramework.MdocLib.Reader;
using WalletFramework.MdocLib.Security;
using WalletFramework.MdocLib.Security.Cose;
using static WalletFramework.MdocLib.Ble.BleRetrievalOptionsFun;

namespace WalletFramework.IsoProximity.EngagementPhase.Implementations;

public class EngagementService(IAesGcmEncryption aes) : IEngagementService
{
    public async Task<DeviceEngagement> CreateDeviceEngagement(PublicKey mdocPubKey)
    {
        var security = new EngagementSecurity(CoseEllipticCurves.P256, mdocPubKey.ToCoseKey());

        var bleRetrievalOptions = BleRetrievalOptionForCentral;
        var retrievalMethod = new DeviceRetrievalMethod(bleRetrievalOptions);

        return new DeviceEngagement(security, [retrievalMethod]);
    }

    public async Task<ReaderEngagement> CreateReaderEngagement(PublicKey verifierPubKey)
    {
        aes.ResetMessageCounter();
        
        var security = new EngagementSecurity(CoseEllipticCurves.P256, verifierPubKey.ToCoseKey());

        var bleRetrievalOptions = BleRetrievalOptionsForPeripheral;
        var retrievalMethod = new DeviceRetrievalMethod(bleRetrievalOptions);

        return new ReaderEngagement(security, [retrievalMethod]);
    }
}
