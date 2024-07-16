using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json.Converters;
using WalletFramework.MdocLib.Common;

namespace WalletFramework.MdocLib;

[JsonConverter(typeof(ValueTypeJsonConverter<NameSpace>))]
public readonly struct NameSpace
{
    public string Value { get; }

    private NameSpace(string value) => Value = value;
    
    public static implicit operator string(NameSpace nameSpace) => nameSpace.ToString();

    internal static Validation<NameSpace> ValidNameSpace(CBORObject nameSpace)
    {
        string str;
        try
        {
            str = nameSpace.AsString();
        }
        catch (Exception e)
        {
            return new CborIsNotATextStringError("nameSpace", e);
        }

        if (string.IsNullOrEmpty(str))
        {
            return new CborValueIsNullOrEmptyError("nameSpace");
        }
        else
        {
            return new NameSpace(str);
        }
    }

    public static Validation<NameSpace> ValidNameSpace(JToken nameSpace)
    {
        var str = nameSpace.ToString();
        if (string.IsNullOrWhiteSpace(str))
        {
            return new NameSpaceIsNullOrEmptyError();
        }
        else
        {
            return new NameSpace(str);
        }
    }

    public override string ToString() => Value;
}
