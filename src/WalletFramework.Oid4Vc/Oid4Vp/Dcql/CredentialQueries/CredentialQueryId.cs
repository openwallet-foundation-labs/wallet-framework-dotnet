using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;

public record CredentialQueryId
{
    private readonly string _value;

    public string AsString() => _value;
    
    private CredentialQueryId(string value) => _value = value;
    
    public static implicit operator string(CredentialQueryId credentialQueryId) => credentialQueryId._value;
    
    public static Validation<CredentialQueryId> Create(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? new StringIsNullOrWhitespaceError<CredentialQueryId>()
            : new CredentialQueryId(value);
} 
