using LanguageExt;
using Newtonsoft.Json;
using WalletFramework.Core.Json.Converters;

namespace WalletFramework.MdocVc;

[JsonConverter(typeof(ValueTypeJsonConverter<MdocName>))]
public readonly struct MdocName
{
    private string Value { get; }

    private MdocName(string value) => Value = value;

    public override string ToString() => Value;

    public static implicit operator string(MdocName mdocName) => mdocName.Value;

    public static Option<MdocName> OptionMdocName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Option<MdocName>.None;

        return new MdocName(name);
    }
}
