using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Common;

namespace WalletFramework.MdocLib;

/// <summary>
///     A CBOR object which is a byte string which is either CBOR or hex encoded.
/// </summary>
public readonly struct CborByteString
{
    public CBORObject Value { get; }

    public byte[] EncodedBytes => Value.EncodeToBytes();

    public byte[] DecodedBytes => Value.GetByteString();
    
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
