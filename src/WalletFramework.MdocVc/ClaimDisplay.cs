using LanguageExt;
using WalletFramework.Core.Localization;

namespace WalletFramework.MdocVc;

public record ClaimDisplay(
    Option<ClaimName> Name, 
    Option<Locale> Locale);

public static class ClaimDisplayJsonKeys
{
    public const string ClaimName = "name";
    public const string Locale = "locale";
}
