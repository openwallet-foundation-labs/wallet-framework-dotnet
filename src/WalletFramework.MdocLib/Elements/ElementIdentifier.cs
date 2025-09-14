using Newtonsoft.Json.Linq;
using PeterO.Cbor;
using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Elements;

public readonly record struct ElementIdentifier
{
    public string Value { get; }

    private ElementIdentifier(string value) => Value = value;

    public static implicit operator string(ElementIdentifier elementIdentifier) => elementIdentifier.Value;

    public override string ToString() => Value;

    internal static Validation<ElementIdentifier> ValidElementIdentifier(CBORObject cbor)
    {
        try
        {
            return new ElementIdentifier(cbor.AsString());
        }
        catch (Exception e)
        {
            return new CborIsNotATextStringError(cbor.ToString(), e);
        }
    }

    public static Validation<ElementIdentifier> ValidElementIdentifier(JToken token)
    {
        var str = token.ToString();
        return ValidElementIdentifier(str);
    }
    
    public static Validation<ElementIdentifier> ValidElementIdentifier(string elementIdentifier)
    {
        if (string.IsNullOrWhiteSpace(elementIdentifier))
        {
            return new ElementIdentifierIsNullOrEmptyError();
        }
        else
        {
            return new ElementIdentifier(elementIdentifier);
        }
    }
}
