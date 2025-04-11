using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;

public readonly struct Sub
{
    private string Value { get; }

    private Sub(string value) => Value = value;

    public override string ToString() => Value;

    public static implicit operator string(Sub sub) => sub.Value;

    public static Validation<Sub> ValidSub(JValue jValue)
    {
        var value = jValue.Value?.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return new StringIsNullOrWhitespaceError<Sub>();
        }

        return new Sub(value);
    }
}
