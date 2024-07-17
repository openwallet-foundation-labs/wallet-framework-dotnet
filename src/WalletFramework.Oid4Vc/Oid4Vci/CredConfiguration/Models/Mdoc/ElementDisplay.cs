using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Localization;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.ElementDisplayFun;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;

public record ElementDisplay
{
    public Option<Locale> Locale { get; }
    
    public Option<ElementName> Name { get; }

    private ElementDisplay(Option<ElementName> name, Option<Locale> locale)
    {
        Name = name;
        Locale = locale;
    }

    public static Option<ElementDisplay> OptionalElementDisplay(JObject display)
    {
        var name = display.GetByKey(NameJsonKey).Match(
            ElementName.OptionalElementName,
            _ => Option<ElementName>.None);
        
        var locale = display.GetByKey(LocaleJsonKey).Match(
            Core.Localization.Locale.OptionLocale,
            _ => Option<Locale>.None);
        
        if (name.IsNone && locale.IsNone)
        {
            return Option<ElementDisplay>.None;
        }

        return new ElementDisplay(name, locale);
    }
}

public static class ElementDisplayFun
{
    public const string LocaleJsonKey = "locale";
    public const string NameJsonKey = "name";

    public static JObject EncodeToJson(this ElementDisplay display)
    {
        var result = new JObject();
        
        display.Locale.IfSome(locale => result.Add(LocaleJsonKey, locale.ToString()));
        display.Name.IfSome(name => result.Add(NameJsonKey, name.ToString()));

        return result;
    }
}
