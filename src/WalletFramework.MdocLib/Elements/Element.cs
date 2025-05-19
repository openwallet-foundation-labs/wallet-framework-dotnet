using PeterO.Cbor;
using WalletFramework.Core.Functional;
using OneOf;
using Newtonsoft.Json.Linq;
using static WalletFramework.MdocLib.Elements.ElementArray;
using static WalletFramework.MdocLib.Elements.ElementMap;

namespace WalletFramework.MdocLib.Elements;

public readonly struct Element
{
    public OneOf<ElementValue, ElementArray, ElementMap> Value { get; }

    private Element(OneOf<ElementValue, ElementArray, ElementMap> value) => Value = value;

    public static implicit operator Element(ElementValue value) => new(value);

    internal static Validation<Element> ValidElement(CBORObject cbor) =>
        cbor.Type switch
        {
            CBORType.Array => ValidElementArray(cbor).OnSuccess(array => new Element(array)),
            CBORType.Map => ValidElementMap(cbor).OnSuccess(map => new Element(map)),
            _ => ElementValue.ValidElementValue(cbor)
                .OnSuccess(singleValue => new Element(singleValue))
        };

    public override string ToString()
    {
        return Value.Match(
            singleValue => singleValue.Value,
            array => array.Value.First().ToString(),
            map => map.Value.First().ToString());
    }

    public JToken ToJToken()
    {
        return Value.Match<JToken>(
            singleValue => singleValue.ToJValue(),
            array => array.ToJArray(),
            map => map.ToJObject()
        );
    }
}
