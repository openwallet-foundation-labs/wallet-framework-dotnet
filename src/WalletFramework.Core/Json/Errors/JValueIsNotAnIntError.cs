using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Json.Errors;

public record JValueIsNotAnIntError : Error
{
    public JValueIsNotAnIntError(string value, Exception e) : base($"The JValue is not an int. Actual value is `{value}`", e)
    {
    }
    
    public JValueIsNotAnIntError(string value) : base($"The JValue is not an int. Actual value is `{value}`")
    {
    }
}
