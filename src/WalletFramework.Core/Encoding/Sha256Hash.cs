using System.Security.Cryptography;
using WalletFramework.Core.Encoding.Errors;
using WalletFramework.Core.Functional;
using static System.Text.Encoding;

namespace WalletFramework.Core.Encoding;

public readonly struct Sha256Hash
{
    private byte[] Value { get; }

    private Sha256Hash(byte[] value)
    {
        Value = value;
    }

    public byte[] AsByteString => Value;

    public override string ToString() => Value.ToString();
    
    public static implicit operator byte[](Sha256Hash sha256Hash) => sha256Hash.Value;

    public static Sha256Hash ComputeHash(byte[] value)
    {
        var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(value);
        return new Sha256Hash(hash);
    }
    
    public static Sha256Hash ComputeHash(byte[] first, byte[] second)
    {
        var sha256 = SHA256.Create();

        var byteString = first.Concat(second).ToArray();
        var hash = sha256.ComputeHash(byteString);
        
        return new Sha256Hash(hash);
    }

    public static Validation<Sha256Hash> ValidHash(byte[] hash)
    {
        if (hash.Length == 0)
        {
            return new InvalidHashLengthError();
        }

        return new Sha256Hash(hash);
    }
}    

