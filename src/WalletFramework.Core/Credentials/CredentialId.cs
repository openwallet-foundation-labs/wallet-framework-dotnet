using WalletFramework.Core.Credentials.Errors;
using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Credentials;

public readonly struct CredentialId
{
    private string Value { get; }

    private CredentialId(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;

    public static implicit operator string(CredentialId credentialId) => credentialId.Value;

    public static CredentialId CreateCredentialId()
    {
        var id = Guid.NewGuid().ToString();
        return new CredentialId(id);
    }

    public static Validation<CredentialId> ValidCredentialId(string id)
    {
        var isValid = Guid.TryParse(id, out _);
        return isValid
            ? new CredentialId(id)
            : new CredentialIdError(id);
    }
}    
