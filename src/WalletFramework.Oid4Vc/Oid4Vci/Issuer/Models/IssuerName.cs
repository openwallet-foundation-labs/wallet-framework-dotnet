using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Json.Converters;

namespace WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;

[JsonConverter(typeof(ValueTypeJsonConverter<IssuerName>))]
public readonly struct IssuerName
{
    private string Value { get; }

    private IssuerName(string value) => Value = value;

    public override string ToString() => Value;
    
    public static implicit operator string(IssuerName issuerName) => issuerName.Value;
    
    public static Option<IssuerName> OptionIssuerName(JToken issuerName)
    {
        var str = issuerName.ToString();
        return string.IsNullOrWhiteSpace(str) 
            ? Option<IssuerName>.None 
            : new IssuerName(str);
    }
}
