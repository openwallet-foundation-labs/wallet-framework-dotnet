using WalletFramework.Core.Functional;
using WalletFramework.Core.Localization;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci.Localization.Samples;

public static class LocaleSample
{
    public static Locale English => Locale.ValidLocale("en-US").UnwrapOrThrow(new InvalidOperationException("The default locale is corrupt."));
    
    public static Locale German => Locale.ValidLocale("de-DE").UnwrapOrThrow(new InvalidOperationException("German locale is corrupt."));
    
    public static Locale Japanese => Locale.ValidLocale("ja-JP").UnwrapOrThrow(new InvalidOperationException("Japanese locale is corrupt."));
    
    public static Locale Korean => Locale.ValidLocale("ko-KR").UnwrapOrThrow(new InvalidOperationException("Korean locale is corrupt."));
}
