using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json.Converters;
using WalletFramework.MdocLib.Common;
using static WalletFramework.MdocLib.Common.Constants;

namespace WalletFramework.MdocLib;

[JsonConverter(typeof(ValueTypeJsonConverter<DocType>))]
public readonly struct DocType
{
    private string Value { get; }

    private DocType(string docType) => Value = docType;

    public override string ToString() => Value;

    public static implicit operator string(DocType docType) => docType.Value;

    internal static Validation<DocType> ValidDoctype(CBORObject cborObject) =>
        cborObject.GetByLabel(DocTypeLabel).OnSuccess(docType =>
        {
            try
            {
                var str = docType.AsString();
                return new DocType(str);
            }
            catch (Exception e)
            {
                return new CborIsNotATextStringError(DocTypeLabel, e).ToInvalid<DocType>();
            }
        });
    
    public static Validation<DocType> ValidDoctype(JToken docType)
    {
        var str = docType.ToString();
        if (string.IsNullOrWhiteSpace(str))
        {
            return new FieldValueIsNullOrEmptyError(DocTypeLabel);
        }
        else
        {
            return new DocType(str);
        }
    }

    public CBORObject Encode() => CBORObject.FromObject(Value);
}
