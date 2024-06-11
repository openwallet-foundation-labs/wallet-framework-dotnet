using Newtonsoft.Json.Linq;
using PeterO.Cbor;
using WalletFramework.Functional;
using WalletFramework.Mdoc.Common;
using static WalletFramework.Mdoc.Common.Constants;

namespace WalletFramework.Mdoc;

public readonly struct DocType
{
    public string Value { get; }

    private DocType(string docType) => Value = docType;

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
