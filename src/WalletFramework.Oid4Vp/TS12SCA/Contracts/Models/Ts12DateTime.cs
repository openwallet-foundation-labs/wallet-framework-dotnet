using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.Oid4Vp.TS12SCA.Contracts.Models;

public sealed record Ts12DateTime
{
    private Ts12DateTime(string value) => AsString = value;

    public string AsString { get; }

    public static Validation<Ts12DateTime> FromJToken(JToken token)
    {
        var value = token.ToString();

        if (string.IsNullOrWhiteSpace(value))
        {
            return new StringIsNullOrWhitespaceError<Ts12DateTime>();
        }

        return new Ts12DateTime(value);
    }
}
