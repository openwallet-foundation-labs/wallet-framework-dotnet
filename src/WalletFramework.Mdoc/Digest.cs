using PeterO.Cbor;
using WalletFramework.Functional;
using WalletFramework.Mdoc.Common;

namespace WalletFramework.Mdoc;

public readonly struct Digest
{
    public byte[] Value { get; }

    private Digest(byte[] value) => Value = value;
    
    public static implicit operator byte[](Digest digest) => digest.Value;

    public static Validation<Digest> ValidDigest(CBORObject digest)
    {
        try
        {
            return new Digest(digest.GetByteString());
        }
        catch (Exception e)
        {
            return new CborIsNotAByteStringError("digest", e);
        }
    }
}
