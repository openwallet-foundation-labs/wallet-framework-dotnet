using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

/// <summary>
///     The format of the credential e.g. SD-JWT(vc+sd-jwt) or Mdoc(mso_mdoc)
/// </summary>
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

public static class FormatFun
{
    public static Format CreateSdJwtFormat() => Format.ValidFormat("vc+sd-jwt").UnwrapOrThrow();
    
    public static Format CreateMdocFormat() => Format.ValidFormat("mso_mdoc").UnwrapOrThrow();
}
