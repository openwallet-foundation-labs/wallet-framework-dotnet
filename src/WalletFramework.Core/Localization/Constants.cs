using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Localization;

public static class Constants
{
    public static Locale DefaultLocale = Locale
        .ValidLocale("en-US")
        .UnwrapOrThrow(new InvalidOperationException("The default locale is corrupt."));
}
