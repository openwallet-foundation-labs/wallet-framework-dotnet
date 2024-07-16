using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

[JsonConverter(typeof(ValueTypeJsonConverter<Format>))]
public readonly struct Format
{
    private string Value { get; }

    private Format(string value) => Value = value;

    public override string ToString() => Value;

    public static implicit operator string(Format format) => format.ToString();

    public static Validation<Format> ValidFormat(JToken format) => format.ToJValue().OnSuccess(value =>
    {
        var str = value.ToString(CultureInfo.InvariantCulture);
        return SupportedFormats.Contains(str)
            ? new Format(str)
            : new FormatNotSupportedError(str).ToInvalid<Format>();
    });

    private static List<string> SupportedFormats => new()
    {
        "vc+sd-jwt",
        "mso_mdoc"
    };
}
