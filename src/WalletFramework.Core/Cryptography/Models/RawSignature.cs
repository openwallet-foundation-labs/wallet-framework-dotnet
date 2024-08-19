using Org.BouncyCastle.Asn1;
using WalletFramework.Core.Cryptography.Errors;
using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Cryptography.Models;

public readonly struct RawSignature
{
    private byte[] Value { get; }

    public byte[] AsByteArray => Value;

    public RawSignature(byte[] value)
    {
        Value = value;
    }

    public static implicit operator byte[](RawSignature signature) => signature.AsByteArray;

    public static Validation<RawSignature> FromDerSignature(byte[] derSignature)
    {
        try
        {
            var seq = (Asn1Sequence)Asn1Object.FromByteArray(derSignature);
            var r = ((DerInteger)seq[0]).Value;
            var s = ((DerInteger)seq[1]).Value;
            var rBytes = r.ToByteArrayUnsigned();
            var sBytes = s.ToByteArrayUnsigned();
            rBytes = PadTo32Bytes(rBytes);
            sBytes = PadTo32Bytes(sBytes);
            
            var signatureBytes = rBytes.Concat(sBytes).ToArray();
            return new RawSignature(signatureBytes);
        }
        catch (Exception e)
        {
            return new InvalidSignatureError("The signature could not be transformed to RAW format", e);
        }
    }
        
    private static byte[] PadTo32Bytes(byte[] value)
    {
        if (value.Length == 32)
            return value;
            
        if (value.Length > 32)
            throw new ArgumentException("Value is too large to fit in 32 bytes");
            
        var padded = new byte[32];
        Array.Copy(value, 0, padded, 32 - value.Length, value.Length);
        return padded;
    }
}
