using System.Globalization;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

public readonly struct Scope
{
    private string Value { get; }
    
    private Scope(string value) => Value = value;
    
    public override string ToString() => Value;
    
    public static implicit operator string(Scope scope) => scope.ToString();

    public static Option<Scope> OptionalScope(JToken scope) => scope.ToJValue().ToOption().OnSome(value =>
    {
        var str = value.ToString(CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(str))
        {
            return Option<Scope>.None;
        }

        return new Scope(str);
    });
}
