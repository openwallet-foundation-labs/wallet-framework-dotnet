using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vp.TransactionDatas;

namespace WalletFramework.Oid4Vp.TS12SCA.Contracts.Models;

public sealed record Ts12PaymentTransactionData(
    TransactionDataProperties TransactionDataProperties,
    Ts12Payment Payment)
{
    public static Validation<TransactionData> FromJObject(
        JObject jObject,
        TransactionDataProperties transactionDataProperties) =>
        from payloadToken in jObject.GetByKey("payload")
        from payloadObject in payloadToken.ToJObject()
        from payment in Ts12Payment.FromJObject(payloadObject)
        let transactionData = new Ts12PaymentTransactionData(transactionDataProperties, payment)
        select TransactionData.WithTs12PaymentTransactionData(transactionData);
}
