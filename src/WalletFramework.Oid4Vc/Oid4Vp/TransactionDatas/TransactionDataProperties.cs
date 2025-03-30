using Newtonsoft.Json.Linq;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;

public record TransactionDataProperties(
    TransactionDataType Type,
    List<TransactionDataCredentialId> CredentialIds,
    List<TransactionDataHashesAlg> TransactionDataHashesAlg,
    Base64UrlString Encoded)
{
    public static Validation<TransactionDataProperties> FromJObject(JObject jObject, Base64UrlString encoded)
    {
        var typesValidation =
            from jToken in jObject.GetByKey("type")
            from type in TransactionDataType.FromJToken(jToken)
            select type;

        var idsValidation =
            from jToken in jObject.GetByKey("credential_ids")
            from jArray in jToken.ToJArray()
            from credentialIds in jArray.TraverseAll(TransactionDataCredentialId.FromJToken)
            select credentialIds.ToList();
        
        var hashesAlgValidation =
            from jToken in jObject.GetByKey("transaction_data_hashes_alg")
            from jArray in jToken.ToJArray()
            from hashesAlgs in jArray.TraverseAll(TransactionDatas.TransactionDataHashesAlg.FromJToken)
            select hashesAlgs.ToList();

        var dataHashesAlgs = hashesAlgValidation.Match(
            algs => algs,
            _ => [TransactionDatas.TransactionDataHashesAlg.Sha256]);

        return
            from transactionDataType in typesValidation
            from credentialIds in idsValidation
            select new TransactionDataProperties(transactionDataType, credentialIds, dataHashesAlgs, encoded);
    }
}
