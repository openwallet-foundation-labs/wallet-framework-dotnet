using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Colors;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Localization;
using WalletFramework.MdocLib;
using WalletFramework.MdocLib.Elements;
using static WalletFramework.MdocVc.Display.Serialization.MdocDisplaySerializationConstants;

namespace WalletFramework.MdocVc.Display.Serialization;

public static class MdocDisplaySerializer
{
    public static string Serialize(MdocDisplay display)
    {
        var json = EncodeToJson(display);
        return json.ToString();
    }

    public static Validation<MdocDisplay> Deserialize(string jsonString)
    {
        var json = JObject.Parse(jsonString);
        return DecodeFromJson(json);
    }

    private static JObject EncodeToJson(this MdocDisplay display)
    {
        var result = new JObject();
        display.Logo.IfSome(logo => result.Add(LogoJsonKey, logo.ToString()));
        display.Name.IfSome(name => result.Add(NameJsonKey, name.ToString()));
        display.BackgroundColor.IfSome(color => result.Add(BackgroundColorJsonKey, color.ToString()));
        display.TextColor.IfSome(color => result.Add(TextColorJsonKey, color.ToString()));
        display.Locale.IfSome(locale => result.Add(LocaleJsonKey, locale.ToString()));
        display.ClaimsDisplays.IfSome(claimsDisplays =>
        {
            var claimsDict = new JObject();
            foreach (var (nameSpace, elementDict) in claimsDisplays)
            {
                var elements = new JObject();
                foreach (var (elementId, claimDisplays) in elementDict)
                {
                    var displays = new JArray();
                    foreach (var claimDisplay in claimDisplays)
                    {
                        var claimDisplayJson = new JObject();
                        claimDisplay.Name.IfSome(
                            name => claimDisplayJson.Add(ClaimDisplayJsonKeys.ClaimName, name.ToString())
                        );
                        claimDisplay.Locale.IfSome(
                            locale => claimDisplayJson.Add(ClaimDisplayJsonKeys.Locale, locale.ToString())
                        );
                        displays.Add(claimDisplayJson);
                    }
                    elements.Add(elementId.ToString(), displays);
                }
                claimsDict.Add(nameSpace.ToString(), elements);
            }
            result.Add(ClaimsDisplaysJsonKey, claimsDict);
        });
        return result;
    }

    private static MdocDisplay DecodeFromJson(JObject display)
    {
        var logo =
            from jToken in display.GetByKey(LogoJsonKey).ToOption()
            let uri = new Uri(jToken.ToString())
            select new MdocLogo(uri);

        var mdocName =
            from jToken in display.GetByKey(NameJsonKey).ToOption()
            from name in MdocName.OptionMdocName(jToken.ToString())
            select name;

        var backgroundColor =
            from jToken in display.GetByKey(BackgroundColorJsonKey).ToOption()
            from color in Color.OptionColor(jToken.ToString())
            select color;

        var textColor =
            from jToken in display.GetByKey(TextColorJsonKey).ToOption()
            from color in Color.OptionColor(jToken.ToString())
            select color;

        var locale =
            from jToken in display.GetByKey(LocaleJsonKey).ToOption()
            from l in Locale.OptionLocale(jToken.ToString())
            select l;

        var claimsDisplays =
            from jToken in display.GetByKey(ClaimsDisplaysJsonKey).ToOption()
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
