using PeterO.Cbor;
using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Cbor;

/// <summary>
///     A CBOR object which is a byte string which is additionally CBOR encoded.
/// </summary>
// TODO: Refactor into tagged (wrapped) byte string and untagged (non-wrapped) 
public readonly struct CborByteString
{
    private CBORObject Value { get; }

    public byte[] EncodedBytes => Value.EncodeToBytes();

    private byte[] DecodedBytes => Value.GetByteString();

    private CborByteString(CBORObject value) => Value = value;

    public CBORObject Decode() => CBORObject.DecodeFromBytes(DecodedBytes);

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
        var byteString = cbor.EncodeToBytes();
        var encodedByteString = CBORObject.FromObject(byteString);

        return CborByteString
            .ValidCborByteString(encodedByteString)
            .UnwrapOrThrow(new InvalidOperationException("CborByteString implementation is corrupt"));
    }
    
    public static CborByteString ToTaggedCborByteString(this CBORObject cbor)
    {
        CBORObject encodedByteString = cbor.ToCborByteString();
        var wrappedByteString = CBORObject.FromObjectAndTag(encodedByteString.EncodeToBytes(), 24);

        return CborByteString
            .ValidCborByteString(wrappedByteString)
            .UnwrapOrThrow(new InvalidOperationException("CborByteString implementation is corrupt"));
    }
}
