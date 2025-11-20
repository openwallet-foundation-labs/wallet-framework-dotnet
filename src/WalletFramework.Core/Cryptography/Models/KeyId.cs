using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.Core.Cryptography.Models;

public readonly struct KeyId
{
    private string Value { get; }

    private KeyId(string value) => Value = value;

    public string AsString() => Value;

    public override string ToString() => Value;

    public static implicit operator string(KeyId keyId) => keyId.Value;

    public static Validation<KeyId> ValidKeyId(string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            return new StringIsNullOrWhitespaceError<KeyId>();
        }

        return new KeyId(keyId);
    }

    public static KeyId CreateKeyId()
    {
        var id = Guid.NewGuid().ToString();
        return new KeyId(id);
    }
}
