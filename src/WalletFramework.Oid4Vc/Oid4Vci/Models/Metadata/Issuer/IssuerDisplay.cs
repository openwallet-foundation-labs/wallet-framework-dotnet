using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Localization;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;
using static WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models.IssuerName;
using static WalletFramework.Core.Localization.Locale;
using static WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer.IssuerLogo;
using static WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer.IssuerDisplayFun;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer;

/// <summary>
///     Represents the visual representations for the Issuer.
/// </summary>
public record IssuerDisplay
{
    /// <summary>
    ///     Gets the name of the Issuer
    /// </summary>
    public Option<IssuerName> Name { get; }

    /// <summary>
    ///     Gets the locale which represents the specific culture or region
    /// </summary>
    public Option<Locale> Locale { get; }
        
    /// <summary>
    ///     Gets the logo of the Issuer
    /// </summary>
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
        var name = jObject.GetByKey(NameJsonKey).ToOption().OnSome(OptionIssuerName);
        var locale = jObject.GetByKey(LocaleJsonKey).OnSuccess(ValidLocale).ToOption();
        var logo = jObject.GetByKey(LogoJsonKey).ToOption().OnSome(OptionalIssuerLogo);

        if (name.IsNone && locale.IsNone && logo.IsNone)
            return Option<IssuerDisplay>.None;

        return new IssuerDisplay(name, locale, logo);
    });
}

public static class IssuerDisplayFun
{
    public const string NameJsonKey = "name";
    public const string LocaleJsonKey = "locale";
    public const string LogoJsonKey = "logo";
    
    public static JObject EncodeToJson(this IssuerDisplay display)
    {
        var json = new JObject();
        display.Name.IfSome(name => json.Add(NameJsonKey, name.ToString()));
        display.Locale.IfSome(locale => json.Add(LocaleJsonKey, locale.ToString()));
        display.Logo.IfSome(logo => json.Add(LogoJsonKey, logo.EncodeToJson()));
        return json;
    }
}
