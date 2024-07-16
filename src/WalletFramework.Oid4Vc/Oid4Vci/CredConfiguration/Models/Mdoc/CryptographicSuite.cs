using System.Globalization;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;

[JsonConverter(typeof(ValueTypeJsonConverter<CryptographicSuite>))]
public readonly struct CryptographicSuite
{
    // TODO: Validate if Value is part of IANA Registry
    public string Value { get; }
        
    private CryptographicSuite(string value) => Value = value;

    public override string ToString() => Value;

    public static Validation<CryptographicSuite> ValidCryptographicSuite(JToken suite) => suite.ToJValue().OnSuccess(value =>
    {
        var str = value.ToString(CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(str))
        {
            return new StringIsNullOrWhitespaceError<CryptographicSuite>().ToInvalid<CryptographicSuite>();
        }
            
        return new CryptographicSuite(str);
    });
}
