using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.Oid4Vc.Payment;

public readonly struct Currency
{
    private Currency(string value)
    {
        Value = value;
    }
    
    private string Value { get; }
    
    public string AsString => Value;

    public static Validation<Currency> FromString(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            return new StringIsNullOrWhitespaceError<Currency>();
        }
        else
        {
            return new Currency(currency);
        }
    }
}
