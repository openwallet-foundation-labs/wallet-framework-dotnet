using PeterO.Cbor;
using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Cbor;

/// <summary>
///     A CBOR encoded byte string which can be tagged or untagged
/// </summary>
public readonly struct CborByteString
{
    private CBORObject Value { get; }

    private CborByteString(CBORObject value) => Value = value;

    public CBORObject Decode() => CBORObject.DecodeFromBytes(Value.GetByteString());

    public CBORObject AsCbor => Value;

    public static implicit operator CBORObject(CborByteString cborByteString) => cborByteString.Value;
    
    public static Validation<CborByteString> ValidCborByteString(CBORObject cbor)
    {
        try
        {
            var bs = cbor.GetByteString();
            CBORObject.DecodeFromBytes(bs);
            return new CborByteString(cbor);
        }
        catch (Exception e)
        {
            return new InvalidCborByteStringError(cbor.ToString(), e);
        }
    }
}

public static class CborByteStringFun
{
    public static CborByteString ToCborByteString(this CBORObject cbor)
    {
        var encodedByteString = CBORObject.FromObject(cbor.EncodeToBytes());
        return CborByteString
            .ValidCborByteString(encodedByteString)
            .UnwrapOrThrow(new InvalidOperationException("CborByteString implementation is corrupt"));
    }
    
    public static CborByteString ToTaggedCborByteString(this CBORObject cbor)
    {
        var wrappedByteString = CBORObject.FromObjectAndTag(cbor.EncodeToBytes(), 24);
        return CborByteString
            .ValidCborByteString(wrappedByteString)
            .UnwrapOrThrow(new InvalidOperationException("CborByteString implementation is corrupt"));
    }
}
