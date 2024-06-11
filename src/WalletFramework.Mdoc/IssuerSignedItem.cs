using Newtonsoft.Json.Linq;
using OneOf;
using PeterO.Cbor;
using WalletFramework.Functional;
using WalletFramework.Mdoc.Common;
using static WalletFramework.Mdoc.ElementArray;
using static WalletFramework.Mdoc.ElementMap;
using static WalletFramework.Mdoc.ElementIdentifier;
using static WalletFramework.Mdoc.ElementValue;
using static WalletFramework.Mdoc.CborByteString;
using static WalletFramework.Mdoc.DigestId;
using static WalletFramework.Mdoc.Random;
using static WalletFramework.Functional.ValidationFun;

namespace WalletFramework.Mdoc;

public readonly struct IssuerSignedItem
{
    public CborByteString ByteString { get; }

    public Random RandomValue { get; }

    public ElementIdentifier ElementId { get; }

    public ElementValue ElementValue { get; }

    public DigestId DigestId { get; }

    private IssuerSignedItem(
        CborByteString byteString,
        DigestId digestId,
        Random randomValue,
        ElementIdentifier elementId,
        ElementValue elementValue)
    {
        ByteString = byteString;
        DigestId = digestId;
        RandomValue = randomValue;
        ElementId = elementId;
        ElementValue = elementValue;
    }

    private static IssuerSignedItem Create(
        CborByteString byteString,
        DigestId digestId,
        Random randomValue,
        ElementIdentifier elementId,
        ElementValue value) =>
        new(byteString, digestId, randomValue, elementId, value);

    internal static Validation<IssuerSignedItem> ValidIssuerSignedItem(CBORObject issuerSignedItem) =>
        ValidCborByteString(issuerSignedItem).OnSuccess(byteString =>
        {
            var issuerSignedItemDecoded = byteString.Decode();

            return 
                Valid(Create)
                .Apply(byteString)
                .Apply(issuerSignedItemDecoded.GetByLabel("digestID").OnSuccess(ValidDigestId))
                .Apply(issuerSignedItemDecoded.GetByLabel("random").OnSuccess(ValidRandom))
                .Apply(issuerSignedItemDecoded.GetByLabel("elementIdentifier").OnSuccess(ValidElementIdentifier))
                .Apply(issuerSignedItemDecoded.GetByLabel("elementValue").OnSuccess(ValidElementValue));
        });
}

public readonly struct ElementIdentifier
{
    public string Value { get; }

    private ElementIdentifier(string value) => Value = value;

    public static implicit operator string(ElementIdentifier elementIdentifier) => elementIdentifier.Value;

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
        if (string.IsNullOrWhiteSpace(str))
        {
            return new ElementIdentifierIsNullOrEmptyError();
        }
        else
        {
            return new ElementIdentifier(str);
        }
    }
}

public readonly struct ElementSingleValue
{
    public string Value { get; }

    private ElementSingleValue(string value) => Value = value;

    internal static Validation<ElementSingleValue> ValidElementSingleValue(CBORObject cbor)
    {
        if (cbor.Type is CBORType.TextString)
        {
            try
            {
                return new ElementSingleValue(cbor.AsString());
            }
            catch (Exception e)
            {
                return new CborIsNotATextStringError(cbor.ToString(), e);
            }
        }
        else
        {
            return new ElementSingleValue(cbor.ToString());
        }
    }
}

public readonly struct ElementArray
{
    public List<ElementValue> Value { get; }

    private ElementArray(List<ElementValue> value) => Value = value;
    
    public ElementValue this[ushort index] => Value[index];

    internal static Validation<ElementArray> ValidElementArray(CBORObject cbor) =>
        cbor.Values
            .Select(ValidElementValue)
            .Traverse(value => value)
            .OnSuccess(values => new ElementArray(values.ToList()));
}

public readonly struct ElementMap
{
    public Dictionary<ElementIdentifier, ElementValue> Value { get; }

    private ElementMap(Dictionary<ElementIdentifier, ElementValue> value) => Value = value;
    
    public ElementValue this[ElementIdentifier key] => Value[key];

    internal static Validation<ElementMap> ValidElementMap(CBORObject cbor)
    {
        try
        {
            return cbor
                .ToDictionary(ValidElementIdentifier, ValidElementValue)
                .OnSuccess(dict => new ElementMap(dict));
        }
        catch (Exception e)
        {
            return new CborIsNotAMapOrAnArrayError(cbor.ToString(), e);
        }
    }
}

public readonly struct ElementValue
{
    public OneOf<ElementSingleValue, ElementArray, ElementMap> Value { get; }

    private ElementValue(OneOf<ElementSingleValue, ElementArray, ElementMap> value) => Value = value;

    public static implicit operator ElementValue(ElementSingleValue singleValue) => new(singleValue);

    internal static Validation<ElementValue> ValidElementValue(CBORObject cbor) =>
        cbor.Type switch
        {
            CBORType.Array => ValidElementArray(cbor).OnSuccess(array => new ElementValue(array)),
            CBORType.Map => ValidElementMap(cbor).OnSuccess(map => new ElementValue(map)),
            _ => ElementSingleValue.ValidElementSingleValue(cbor)
                .OnSuccess(singleValue => new ElementValue(singleValue))
        };
}

public readonly struct Random
{
    public byte[] Value { get; }

    private Random(byte[] value) => Value = value;

    internal static Validation<Random> ValidRandom(CBORObject cbor) => 
        cbor.TryGetByteString().OnSuccess(bytes => new Random(bytes));
}
