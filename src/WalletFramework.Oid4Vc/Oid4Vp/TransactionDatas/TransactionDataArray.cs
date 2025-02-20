using Newtonsoft.Json.Linq;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas.Errors;
using WalletFramework.Oid4Vc.Payment;

namespace WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;

public record TransactionDataArray(Base64UrlString[] EncodedTransactionDataStrings)
{
    public static Validation<TransactionDataArray> FromJObject(JArray jArray)
    {
        var arrayValidation = jArray.TraverseAll(jToken =>
        {
            return Base64UrlString.FromString(jToken.ToString());
        });

        return
            from base64UrlStrings in arrayValidation
            select new TransactionDataArray(base64UrlStrings.ToArray());
    }
}

public static class TransactionDataArrayFun
{
    public static Validation<IEnumerable<PaymentTransactionData>> Decode(this TransactionDataArray array) =>
        array.EncodedTransactionDataStrings.TraverseAll(PaymentTransactionData.FromBase64Url).OnSuccess(datas =>
        {
            var list = datas.ToList();
            if (list.Count > 2)
            {
                return new InvalidTransactionDataError(
                    $"Multiple instances of {nameof(PaymentTransactionData)} found");
            }
            else
            {
                return ValidationFun.Valid<IEnumerable<PaymentTransactionData>>(list);
            }
        });
}
