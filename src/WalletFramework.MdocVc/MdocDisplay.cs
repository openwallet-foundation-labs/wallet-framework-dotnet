using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Colors;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;
using WalletFramework.Core.Localization;
using WalletFramework.MdocLib;

namespace WalletFramework.MdocVc;

public record MdocDisplay(
    [property: JsonProperty(MdocDisplayJsonKeys.Logo)]
    [property: JsonConverter(typeof(OptionJsonConverter<MdocLogo>))]
    Option<MdocLogo> Logo,
    [property: JsonProperty(MdocDisplayJsonKeys.Name)]
    [property: JsonConverter(typeof(OptionJsonConverter<MdocName>))]
    Option<MdocName> Name,
    [property: JsonProperty(MdocDisplayJsonKeys.BackgroundColor)]
    [property: JsonConverter(typeof(OptionJsonConverter<Color>))]
    Option<Color> BackgroundColor,
    [property: JsonProperty(MdocDisplayJsonKeys.TextColor)]
    [property: JsonConverter(typeof(OptionJsonConverter<Color>))]
    Option<Color> TextColor,
    [property: JsonProperty(MdocDisplayJsonKeys.Locale)]
    [property: JsonConverter(typeof(OptionJsonConverter<Locale>))]
    Option<Locale> Locale,
    [property: JsonProperty(MdocDisplayJsonKeys.ClaimsDisplays)]
    [property: JsonConverter(typeof(OptionJsonConverter<Dictionary<NameSpace, Dictionary<ElementIdentifier, List<ClaimDisplay>>>>))]
    Option<Dictionary<NameSpace, Dictionary<ElementIdentifier, List<ClaimDisplay>>>> ClaimsDisplays);

public static class MdocDisplayJsonKeys
{
    public const string Logo = "logo";
    public const string Name = "name";
    public const string BackgroundColor = "background_color";
    public const string TextColor = "text_color";
    public const string Locale = "locale";
    public const string ClaimsDisplays = "claims_displays";
}

public static class MdocDisplayFun
{
    public static Option<MdocDisplay> GetByLocale(this List<MdocDisplay> displays, Locale locale)
    {
        var dict = new Dictionary<Locale, MdocDisplay>();
        foreach (var display in displays)
        {
            display.Locale.Match(
                displayLocale =>
                {
                    dict.Add(displayLocale, display);
                },
                () =>
                {
                    if (!dict.Keys.Contains(Constants.DefaultLocale))
                    {
                        dict.Add(Constants.DefaultLocale, display);
                    }
                }
            );
        }

        if (dict.Any())
        {
            return dict.FindOrDefault(locale);
        }
        else
        {
            return Option<MdocDisplay>.None;
        }
    }
    
    public static Option<List<MdocDisplay>> DecodeFromJson(JArray array)
    {
        var result = array.TraverseAny(token => 
            from jObject in token.ToJObject()
            select DecodeFromJson(jObject)
        ).ToOption();

        return
            from displays in result
            select displays.ToList();
    }

    private static MdocDisplay DecodeFromJson(JObject display)
    {
        var logo =
            from jToken in display.GetByKey(MdocDisplayJsonKeys.Logo).ToOption()
            let uri = new Uri(jToken.ToString())
            select new MdocLogo(uri);
        
        var mdocName =
            from jToken in display.GetByKey(MdocDisplayJsonKeys.Name).ToOption()
            from name in MdocName.OptionMdocName(jToken.ToString())
            select name;

        var backgroundColor =
            from jToken in display.GetByKey(MdocDisplayJsonKeys.BackgroundColor).ToOption()
            from color in Color.OptionColor(jToken.ToString())
            select color;
        
        var textColor =
            from jToken in display.GetByKey(MdocDisplayJsonKeys.TextColor).ToOption()
            from color in Color.OptionColor(jToken.ToString())
            select color;
        
        var locale =
            from jToken in display.GetByKey(MdocDisplayJsonKeys.Locale).ToOption()
            from l in Locale.OptionLocale(jToken.ToString())
            select l;

        var claimsDisplays =
            from jToken in display.GetByKey(MdocDisplayJsonKeys.ClaimsDisplays).ToOption()
            from claimsJson in jToken.ToJObject().ToOption()
            from displays in DecodeClaimsDisplaysFromJson(claimsJson)
            select displays;

        return new MdocDisplay(logo, mdocName, backgroundColor, textColor, locale, claimsDisplays);
    }

    private static Option<Dictionary<NameSpace, Dictionary<ElementIdentifier, List<ClaimDisplay>>>>
        DecodeClaimsDisplaysFromJson(JObject namespaceDict)
    {
        var result = new Dictionary<NameSpace, Dictionary<ElementIdentifier, List<ClaimDisplay>>>();

        var tuples = namespaceDict.Properties().Select(prop =>
        {
            var claimsDict =
                from jObject in prop.Value.ToJObject().ToOption()
                from claimsDisplays in DecodeClaimsDisplaysDictFromJson(jObject)
                select claimsDisplays;
            
            return (
                NameSpace: NameSpace.ValidNameSpace(prop.Name).ToOption(),
                ClaimsDict: claimsDict
            );
        });
        
        foreach (var (nameSpace, claimsDict) in tuples)
        {
            nameSpace.OnSome(space => claimsDict.OnSome(dictionary =>
            {
                result.Add(space, dictionary);
                return Unit.Default;
            }));
        }

        return result.Any() 
            ? result
            : Option<Dictionary<NameSpace, Dictionary<ElementIdentifier, List<ClaimDisplay>>>>.None;
    } 

    private static Option<Dictionary<ElementIdentifier, List<ClaimDisplay>>>
        DecodeClaimsDisplaysDictFromJson(JObject json)
    {
        var result = new Dictionary<ElementIdentifier, List<ClaimDisplay>>();
        
        var tuples = json.Properties().Select(prop =>
        {
            var displays =
                from jArray in prop.Value.ToJArray().ToOption()
                from claimDisplays in jArray.TraverseAny(token =>
                {
                    var optionName = 
                        from jToken in token.GetByKey(ClaimDisplayJsonKeys.ClaimName).ToOption()
                        from name in ClaimName.OptionClaimName(jToken.ToString())
                        select name;
            
                    var optionLocale = 
                        from jToken in token.GetByKey(ClaimDisplayJsonKeys.Locale).ToOption()
                        from locale in Locale.OptionLocale(jToken.ToString())
                        select locale;
        
                    return
                        from name in optionName
                        from locale in optionLocale
                        select new ClaimDisplay(name, locale);
                })
                select claimDisplays.ToList();

            return (
                Id: ElementIdentifier.ValidElementIdentifier(prop.Name).ToOption(),
                Displays: displays
            );
        });    
        
        foreach (var (elementId, claimDisplays) in tuples)
        {
            elementId.OnSome(id => claimDisplays.OnSome(displays =>
            {
                result.Add(id, displays);
                return Unit.Default;
            }));
        }
        
        return result.Any() 
            ? result
            : Option<Dictionary<ElementIdentifier, List<ClaimDisplay>>>.None;
    }
}
