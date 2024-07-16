using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;
using WalletFramework.Core.Localization;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;

public record ElementDisplay
{
    [JsonProperty("locale")]
    [JsonConverter(typeof(OptionJsonConverter<Locale>))]
    public Option<Locale> Locale { get; }
    
    [JsonProperty("name")]
    [JsonConverter(typeof(OptionJsonConverter<ElementName>))]
    public Option<ElementName> Name { get; }

    private ElementDisplay(Option<ElementName> name, Option<Locale> locale)
    {
        Name = name;
        Locale = locale;
    }

    public static Option<ElementDisplay> OptionalElementDisplay(JObject display)
    {
        var name = display.GetByKey("name").Match(
            ElementName.OptionalElementName,
            _ => Option<ElementName>.None);
        
        var locale = display.GetByKey("locale").Match(
            Core.Localization.Locale.OptionLocale,
            _ => Option<Locale>.None);
        
        if (name.IsNone && locale.IsNone)
        {
            return Option<ElementDisplay>.None;
        }

        return new ElementDisplay(name, locale);
    }
}
