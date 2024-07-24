using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;

namespace WalletFramework.MdocLib.Security;

public readonly struct Random
{
    public byte[] Value { get; }

    private Random(byte[] value) => Value = value;

    internal static Validation<Random> ValidRandom(CBORObject cbor) => 
        cbor.TryGetByteString().OnSuccess(bytes => new Random(bytes));
}
