using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;

namespace WalletFramework.Oid4Vc.Payment;

public record PaymentTransactionData(TransactionDataProperties TransactionDataProperties, PaymentData PaymentData)
{
    public static Validation<TransactionData> FromJObject(
        JObject jObject,
        TransactionDataProperties transactionDataProperties) =>
        from paymentDataToken in jObject.GetByKey("payment_data")
        from paymentDataJObject in paymentDataToken.ToJObject()
        from paymentData in PaymentData.FromJObject(paymentDataJObject)
        let paymentTransactionData = new PaymentTransactionData(transactionDataProperties, paymentData)
        select new TransactionData(paymentTransactionData);
}
