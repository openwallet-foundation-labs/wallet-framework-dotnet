using PeterO.Cbor;
using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Elements;

public readonly struct ElementValue
{
    public string Value { get; }

    private ElementValue(string value) => Value = value;

    internal static Validation<ElementValue> ValidElementValue(CBORObject cbor)
    {
        if (cbor.Type is CBORType.TextString)
        {
            try
            {
                return new ElementValue(cbor.AsString());
            }
            catch (Exception e)
            {
                return new CborIsNotATextStringError(cbor.ToString(), e);
            }
        }
        else
        {
            return new ElementValue(cbor.ToString());
        }
    }
}
