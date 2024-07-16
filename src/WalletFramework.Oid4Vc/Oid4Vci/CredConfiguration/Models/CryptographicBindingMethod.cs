using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

[JsonConverter(typeof(ValueTypeJsonConverter<CryptographicBindingMethod>))]
public readonly struct CryptographicBindingMethod
{
    private string Value { get; }

    private CryptographicBindingMethod(string value) => Value = value;

    public override string ToString() => Value;

    public static implicit operator string(CryptographicBindingMethod method) => method.ToString();
    
    public static Validation<CryptographicBindingMethod> ValidCryptographicBindingMethod(JToken method) => method.ToJValue().OnSuccess(value =>
    {
        var str = value.ToString(CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(str))
        {
            return new StringIsNullOrWhitespaceError<CryptographicBindingMethod>().ToInvalid<CryptographicBindingMethod>();
        }

        return new CryptographicBindingMethod(str);
    });
}
