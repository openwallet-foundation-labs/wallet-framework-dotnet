using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Base64Url.Errors;

public record Base64UrlError : Error
{
    public Base64UrlError(string value, Exception e) 
        : base($"The string is not a valid Base64Url. Actual value is {value}", e)
    {
    }
    
    public Base64UrlError(Exception e) 
        : base("The given bytes could not be parsed to Base64Url", e)
    {
    }
}
