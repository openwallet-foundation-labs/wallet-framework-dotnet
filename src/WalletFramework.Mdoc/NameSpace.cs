using Newtonsoft.Json.Linq;
using PeterO.Cbor;
using WalletFramework.Functional;
using WalletFramework.Mdoc.Common;

namespace WalletFramework.Mdoc;

public readonly struct NameSpace
{
    public string Value { get; }

    private NameSpace(string value) => Value = value;

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
