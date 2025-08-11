using LanguageExt;
using WalletFramework.Core.Colors;
using WalletFramework.Core.Localization;
using WalletFramework.MdocLib;
using WalletFramework.MdocLib.Elements;

namespace WalletFramework.MdocVc.Display;

public record MdocDisplay(
    Option<MdocLogo> Logo,
    Option<MdocName> Name,
    Option<Color> BackgroundColor,
    Option<Color> TextColor,
    Option<Locale> Locale,
    Option<Dictionary<NameSpace, Dictionary<ElementIdentifier, List<ClaimDisplay>>>> ClaimsDisplays);

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
}
