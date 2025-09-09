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
