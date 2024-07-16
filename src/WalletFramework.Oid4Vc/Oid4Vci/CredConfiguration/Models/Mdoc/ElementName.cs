using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Json.Converters;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;

[JsonConverter(typeof(ValueTypeJsonConverter<ElementName>))]
public readonly struct ElementName
{
    private string Value { get; }

    private ElementName(string value) => Value = value;

    public override string ToString() => Value;

    public static implicit operator string(ElementName elementName) => elementName.Value;

    public static Option<ElementName> OptionalElementName(JToken name)
    {
        var str = name.ToString();
        if (string.IsNullOrWhiteSpace(str))
        {
            return Option<ElementName>.None;
        }
        else
        {
            return new ElementName(str);
        }
    }
}
