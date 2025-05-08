using System.Net.NetworkInformation;
using System.Reactive;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Credentials.Errors;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas.Errors;
using Error = OneOf.Types.Error;

namespace WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;

public record TransactionDataArray(Base64UrlString[] EncodedTransactionDataStrings)
{
    public static Validation<TransactionDataArray> FromJArray(JArray jArray)
    {
        var arrayValidation = jArray.TraverseAll(jToken => Base64UrlString.FromString(jToken.ToString()));

        return
            from base64UrlStrings in arrayValidation
            select new TransactionDataArray(base64UrlStrings.ToArray());
    }
}

public static class TransactionDataArrayFun
{
    public static Validation<IEnumerable<TransactionData>> Decode(this TransactionDataArray array)
    {
        return array.EncodedTransactionDataStrings.TraverseAll(TransactionData.FromBase64Url);
    } 
        
}
