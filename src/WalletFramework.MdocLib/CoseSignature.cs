using PeterO.Cbor;
using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib;

public readonly struct CoseSignature
{
    public byte[] AsByteString { get; }
    
    public CoseSignature(byte[] asByteString) => AsByteString = asByteString;

    internal static Validation<CoseSignature> ValidCoseSignature(CBORObject issuerAuth) => issuerAuth
        .GetByIndex(3)
        .OnSuccess(CborFun.TryGetByteString)
        .OnSuccess(bytes => new CoseSignature(bytes));
}

