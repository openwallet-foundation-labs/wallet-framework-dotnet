using Org.BouncyCastle.Crypto.Parameters;
using WalletFramework.MdocLib.Ble.BleUuids;
using WalletFramework.MdocLib.Device;
using WalletFramework.MdocLib.Device.Request;
using WalletFramework.MdocLib.Reader;
using WalletFramework.MdocLib.Security;

namespace WalletFramework.MdocLib.Ble.Abstractions;

public interface IBleCentral
{
    public Task Write(BleUuid serviceUuid, BleUuid characteristicUuid, byte[] data);

    public Task<(DeviceRequest, SessionTranscript)> WaitFor(
        BleUuid serviceUuid,
        BleUuid characteristicUuid,
        ReaderEngagement readerEngagement,
        ECPrivateKeyParameters privateKey,
        DeviceEngagement deviceEngagement);

    public Task Init(BleUuid serviceUuid);
}
