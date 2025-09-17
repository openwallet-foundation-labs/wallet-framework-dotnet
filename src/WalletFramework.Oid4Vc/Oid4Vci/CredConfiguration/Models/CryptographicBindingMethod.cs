using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

public readonly struct CryptographicBindingMethod
{
    private string Value { get; }

    private CryptographicBindingMethod(string value) => Value = value;

    public override string ToString() => Value;

    public static implicit operator string(CryptographicBindingMethod method) => method.ToString();
    
    public static Validation<CryptographicBindingMethod> ValidCryptographicBindingMethod(JToken method) => method.ToJValue().OnSuccess(value =>
    {
        var str = value.ToString(CultureInfo.InvariantCulture);
        return SupportedCryptographicBindingMethods.Contains(str)
            ? new CryptographicBindingMethod(str)
            : new CryptographicBindingMethodNotSupportedError(str).ToInvalid<CryptographicBindingMethod>();
    });
    
    private static List<string> SupportedCryptographicBindingMethods => new()
    {
        "jwt"
    };
}
