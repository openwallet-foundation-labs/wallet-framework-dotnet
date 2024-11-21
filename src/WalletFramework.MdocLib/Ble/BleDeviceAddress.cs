using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.MdocLib.Ble;

public readonly struct BleDeviceAddress
{
    private string Value { get; }

    private BleDeviceAddress(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;

    public static implicit operator string(BleDeviceAddress bleDeviceAddress) => bleDeviceAddress.Value;

    public static Validation<BleDeviceAddress> ValidBleDeviceAdress(string bleDeviceAdress)
    {
        if (string.IsNullOrWhiteSpace(bleDeviceAdress))
            return new StringIsNullOrWhitespaceError<BleDeviceAddress>();

        return new BleDeviceAddress(bleDeviceAdress);
    }
}    

