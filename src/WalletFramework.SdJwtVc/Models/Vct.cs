using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;

namespace WalletFramework.SdJwtVc.Models;

public readonly record struct Vct
{
    private string Value { get; }

    private Vct(string vct) => Value = vct;

    public override string ToString() => Value;
    
    public static implicit operator string(Vct vct) => vct.Value;

    public static Validation<Vct> ValidVct(JToken vct) => vct.ToJValue().OnSuccess(value =>
    {
        var str = value.ToString(CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(str))
            return new StringIsNullOrWhitespaceError<Vct>().ToInvalid<Vct>();

        return new Vct(str);
    });
    
    public static Validation<Vct> ValidVct(string vct)
    {
        if (string.IsNullOrWhiteSpace(vct))
            return new StringIsNullOrWhitespaceError<Vct>().ToInvalid<Vct>();

        return new Vct(vct);
    }
}
