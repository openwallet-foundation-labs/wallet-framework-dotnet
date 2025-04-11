using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;

public readonly struct IssuedAt
{
    public DateTime Value { get; }
    
    private IssuedAt(DateTime value) => Value = value;
    
    public static implicit operator string(IssuedAt issuedAt) => issuedAt.Value.ToString("F");
    
    public static implicit operator DateTime(IssuedAt policyUri) => policyUri.Value;
    
    public static implicit operator IssuedAt(DateTime dateTime) => CreateIssuedAt(dateTime);
    
    public static implicit operator IssuedAt(long timestamp) => CreateIssuedAt(DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime);
    
    public override string ToString() => Value.ToString("F");
    
    private static IssuedAt CreateIssuedAt(DateTime dateTime)
    {
        return new IssuedAt(dateTime);
    }
    
    public static Validation<IssuedAt> ValidIssuedAt(JValue jValue)
    {
        var value = jValue.Value?.ToString();
        if (string.IsNullOrWhiteSpace(value) || !long.TryParse(value, out var issuedAt))
        {
            return new StringIsNullOrWhitespaceError<Sub>();
        }

        return (IssuedAt)issuedAt;
    }
}
