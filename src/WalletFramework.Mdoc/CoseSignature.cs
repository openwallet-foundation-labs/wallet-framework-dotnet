using PeterO.Cbor;
using WalletFramework.Functional;

namespace WalletFramework.Mdoc;

public readonly struct CoseSignature
{
    public byte[] Value { get; }
    
    private CoseSignature(byte[] value) => Value = value;

    internal static Validation<CoseSignature> ValidCoseSignature(CBORObject issuerAuth) => issuerAuth
        .GetByIndex(3)
        .OnSuccess(CborFun.TryGetByteString)
        .OnSuccess(bytes => new CoseSignature(bytes));
}

