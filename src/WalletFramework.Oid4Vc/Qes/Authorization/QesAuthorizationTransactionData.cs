using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;

namespace WalletFramework.Oid4Vc.Qes.Authorization;

public record QesAuthorizationTransactionData(
    List<TransactionDataCredentialId> CredentialIds,
    List<DocumentDigest> DocumentDigests)
{
    public static Validation<TransactionData> FromJObject(
        JObject jObject,
        TransactionDataProperties transactionDataProperties) =>
        from credentialIdsJToken in jObject.GetByKey("credential_ids")
        from credentialIdsJArray in credentialIdsJToken.ToJArray()
        from credentialIds in credentialIdsJArray.TraverseAll(TransactionDataCredentialId.FromJToken)
        from documentDigestsToken in jObject.GetByKey("documentDigests")
        from documentDigestsArray in documentDigestsToken.ToJArray()
        from documentDigests in documentDigestsArray.TraverseAll(DocumentDigest.FromJObject)
        let qesTransactionData = new QesAuthorizationTransactionData(credentialIds.ToList(), documentDigests.ToList())
        select TransactionData.WithQesAuthorizationTransactionData(qesTransactionData, transactionDataProperties);
}
