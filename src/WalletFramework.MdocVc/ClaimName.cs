using LanguageExt;
using Newtonsoft.Json;
using WalletFramework.Core.Json.Converters;

namespace WalletFramework.MdocVc;

[JsonConverter(typeof(ValueTypeJsonConverter<ClaimName>))]
public readonly struct ClaimName
{
    private string Value { get; }

    private ClaimName(string value) => Value = value;

    public override string ToString() => Value;

    public static implicit operator string(ClaimName name) => name.Value;
    
    public static Option<ClaimName> OptionClaimName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Option<ClaimName>.None;

        return new ClaimName(name);
    }
}
