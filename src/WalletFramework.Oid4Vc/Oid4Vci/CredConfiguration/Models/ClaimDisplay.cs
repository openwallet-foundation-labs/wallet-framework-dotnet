using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Localization;
using static WalletFramework.Core.Localization.Locale;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.ClaimDisplayJsonExtensions;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

public record ClaimDisplay
{
    public Option<string> Name { get; }

    public Option<Locale> Locale { get; }
    
    private ClaimDisplay(Option<string> name, Option<Locale> locale)
    {
        Name = name;
        Locale = locale;
    }
    
    private static ClaimDisplay Create(Option<string> name, Option<Locale> locale) => 
        new(name, locale);
    
    public static Validation<ClaimDisplay> ValidClaimDisplay(JToken config)
    {
        var name =
            from jToken in config.GetByKey(NameJsonKey)
            select jToken.ToObject<string>();
        
        var locale =
            from jToken in config.GetByKey(LocaleJsonKey)
            from validLocale in ValidLocale(jToken)
            select validLocale;
        
        return ValidationFun.Valid(Create)
            .Apply(name.ToOption())
            .Apply(locale.ToOption());
    }
}

public static class ClaimDisplayJsonExtensions
{
    public const string NameJsonKey = "name";
    public const string LocaleJsonKey = "locale";
    
    public static JObject EncodeToJson(this ClaimDisplay display)
    {
        var result = new JObject();

        display.Locale.IfSome(locale => result.Add(LocaleJsonKey, locale.ToString()));
        display.Name.IfSome(name => result.Add(NameJsonKey, name));

        return result;
    }
}
