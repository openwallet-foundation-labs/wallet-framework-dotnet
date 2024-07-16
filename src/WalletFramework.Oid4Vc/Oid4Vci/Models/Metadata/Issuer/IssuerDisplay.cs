using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;
using WalletFramework.Core.Localization;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;
using static WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models.IssuerName;
using static WalletFramework.Core.Localization.Locale;
using static WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer.IssuerLogo;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer;

/// <summary>
///     Represents the visual representations for the Issuer.
/// </summary>
public record IssuerDisplay
{
    /// <summary>
    ///     Gets the name of the Issuer
    /// </summary>
    [JsonProperty("name")]
    [JsonConverter(typeof(OptionJsonConverter<IssuerName>))]
    public Option<IssuerName> Name { get; }

    /// <summary>
    ///     Gets the locale which represents the specific culture or region
    /// </summary>
    [JsonProperty("locale")]
    [JsonConverter(typeof(OptionJsonConverter<Locale>))]
    public Option<Locale> Locale { get; }
        
    /// <summary>
    ///     Gets the logo of the Issuer
    /// </summary>
    [JsonProperty("logo")]
    [JsonConverter(typeof(OptionJsonConverter<IssuerLogo>))]
    public Option<IssuerLogo> Logo { get; }
    
    private IssuerDisplay(
        Option<IssuerName> name,
        Option<Locale> locale,
        Option<IssuerLogo> logo)
    {
        Name = name;
        Locale = locale;
        Logo = logo;
    }

    public static Option<IssuerDisplay> OptionalIssuerDisplay(JToken display) => display.ToJObject().ToOption().OnSome(jObject =>
    {
        var name = jObject.GetByKey("name").ToOption().OnSome(OptionIssuerName);
        var locale = jObject.GetByKey("locale").OnSuccess(ValidLocale).ToOption();
        var logo = jObject.GetByKey("logo").ToOption().OnSome(OptionalIssuerLogo);

        if (name.IsNone && locale.IsNone && logo.IsNone)
            return Option<IssuerDisplay>.None;

        return new IssuerDisplay(name, locale, logo);
    });
}
