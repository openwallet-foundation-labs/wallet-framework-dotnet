using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;

namespace WalletFramework.MdocLib.Security.Cose;

public readonly struct CoseSignature
{
    private byte[] Value { get; }

    public byte[] AsByteArray => Value;

    public CBORObject AsCbor => CBORObject.FromObject(AsByteArray);
    
    public CoseSignature(byte[] byteArray) => Value = byteArray;

    internal static Validation<CoseSignature> ValidCoseSignature(CBORObject issuerAuth) => issuerAuth
        .GetByIndex(3)
        .OnSuccess(CborFun.TryGetByteString)
        .OnSuccess(bytes => new CoseSignature(bytes));
}

