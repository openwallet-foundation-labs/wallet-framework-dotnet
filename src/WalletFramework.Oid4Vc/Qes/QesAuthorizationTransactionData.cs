using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;

namespace WalletFramework.Oid4Vc.Qes;

public record QesAuthorizationTransactionData(
    TransactionDataProperties TransactionDataProperties,
    List<DocumentDigest> DocumentDigests)
{
    public static Validation<TransactionData> FromJObject(
        JObject jObject,
        TransactionDataProperties transactionDataProperties) =>
        from documentDigestsToken in jObject.GetByKey("documentDigests")
        from documentDigestsArray in documentDigestsToken.ToJArray()
        from documentDigests in documentDigestsArray.TraverseAll(DocumentDigest.FromJObject)
        let qesTransactionData = new QesAuthorizationTransactionData(transactionDataProperties, documentDigests.ToList())
        select new TransactionData(qesTransactionData);
}
