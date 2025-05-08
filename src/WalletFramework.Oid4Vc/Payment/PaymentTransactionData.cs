using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;

namespace WalletFramework.Oid4Vc.Payment;

public record PaymentTransactionData(List<TransactionDataCredentialId> CredentialIds, PaymentData PaymentData)
{
    public static Validation<TransactionData> FromJObject(
        JObject jObject,
        TransactionDataProperties transactionDataProperties) =>
        from credentialIdsJToken in jObject.GetByKey("credential_ids")
        from credentialIdsJArray in credentialIdsJToken.ToJArray()
        from credentialIds in credentialIdsJArray.TraverseAll(TransactionDataCredentialId.FromJToken)
        from paymentDataToken in jObject.GetByKey("payment_data")
        from paymentDataJObject in paymentDataToken.ToJObject()
        from paymentData in PaymentData.FromJObject(paymentDataJObject)
        let paymentTransactionData = new PaymentTransactionData(credentialIds.ToList(), paymentData)
        select TransactionData.WithPaymentTransactionData(paymentTransactionData, transactionDataProperties);
}
