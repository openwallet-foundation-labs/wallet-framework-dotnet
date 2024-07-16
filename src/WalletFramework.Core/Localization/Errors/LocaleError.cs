using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.Errors;

public record LocaleError : Error
{
    public LocaleError(string value, Exception e) : base($"The locale could not be processed. Value is: `{value}`", e)
    {
    }

    public LocaleError(string value) : base($"The locale could not be processed. Value is: `{value}`")
    {
    }
}
