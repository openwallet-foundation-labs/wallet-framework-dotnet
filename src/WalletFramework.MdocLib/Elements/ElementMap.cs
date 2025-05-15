using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using Newtonsoft.Json.Linq;
using static WalletFramework.MdocLib.Elements.ElementIdentifier;
using static WalletFramework.MdocLib.Elements.Element;

namespace WalletFramework.MdocLib.Elements;

public readonly struct ElementMap
{
    public Dictionary<ElementIdentifier, Element> Value { get; }

    private ElementMap(Dictionary<ElementIdentifier, Element> value) => Value = value;
    
    public Element this[ElementIdentifier key] => Value[key];

    internal static Validation<ElementMap> ValidElementMap(CBORObject cbor)
    {
        try
        {
            return cbor
                .ToDictionary(ValidElementIdentifier, ValidElement)
                .OnSuccess(dict => new ElementMap(dict));
        }
        catch (Exception e)
        {
            return new CborIsNotAMapOrAnArrayError(cbor.ToString(), e);
        }
    }

    public JObject ToJObject()
    {
        var obj = new JObject();
        foreach (var kvp in Value)
        {
            obj[kvp.Key.Value] = kvp.Value.ToJToken();
        }
        return obj;
    }
}
