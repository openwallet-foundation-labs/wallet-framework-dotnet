using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.MdocLib.Ble.BleUuids;

public readonly struct BleUuid
{
    private string Value { get; }

    private BleUuid(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;

    public static implicit operator string(BleUuid bleUuid) => bleUuid.Value;

    public static Validation<BleUuid> FromString(string bleUuid)
    {
        if (string.IsNullOrWhiteSpace(bleUuid))
            return new StringIsNullOrWhitespaceError<BleUuid>();

        return new BleUuid(bleUuid);
    }
    
    public static Validation<BleUuid> FromCbor(CBORObject cbor, string name)
    {
        try
        {
            var uuid = new Guid(cbor.GetByteString());
            return new BleUuid(uuid.ToString());
        }
        catch (Exception e)
        {
            return new CborIsNotAByteStringError(name, e);
        }
    }
}    

public static class BleUuidFun
{
    public static BleUuid CreateServiceUuid()
    {
        var random = Guid.NewGuid();
        return BleUuid.FromString(random.ToString()).UnwrapOrThrow();
    }
}
