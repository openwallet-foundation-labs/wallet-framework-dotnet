using Org.BouncyCastle.Crypto.Parameters;
using WalletFramework.MdocLib.Ble.BleUuids;
using WalletFramework.MdocLib.Reader;

namespace WalletFramework.MdocLib.Ble.Abstractions;

public interface IBlePeripheral
{
    public Task Advertise(BleUuid serviceUuid, ECPrivateKeyParameters privateKey, ReaderEngagement readerEngagement);

    public void StopAdvertising();
}
