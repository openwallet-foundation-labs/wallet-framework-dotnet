using PeterO.Cbor;
using WalletFramework.Functional;

namespace WalletFramework.Mdoc;

public readonly struct DigestId
{
    public uint Value { get; }

    private DigestId(uint value) => Value = value;

    public static Validation<DigestId> ValidDigestId(CBORObject cbor)
    {
        try
        {
            var value = cbor.AsNumber().ToUInt32Checked();
            return new DigestId(value);
        }
        catch (Exception e)
        {
            return new DigestIdIsNotAPositiveIntegerError(cbor.ToString(), e);
        }
    }

    public override string ToString() => Value.ToString();

    public record DigestIdIsNotAPositiveIntegerError(string Value, Exception E) : Error($"DigestID is not a positive integer, Actual is {Value}", E);
}
