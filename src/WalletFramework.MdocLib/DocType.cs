using Newtonsoft.Json.Linq;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using static WalletFramework.MdocLib.Constants;

namespace WalletFramework.MdocLib;

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

    public CBORObject ToCbor() => CBORObject.FromObject(Value);
}
