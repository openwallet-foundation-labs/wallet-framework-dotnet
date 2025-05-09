using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;

namespace WalletFramework.Oid4Vc.Qes.CertCreation;

public record QCertCreationTransactionData(TransactionDataProperties TransactionDataProperties, TermsConditionsUri TermsConditionsUri, string Hash)
{
    public static Validation<TransactionData> FromJObject(
        JObject jObject,
        TransactionDataProperties transactionDataProperties) =>
        from termsConditionsUriToken in jObject.GetByKey("QC_terms_conditions_uri")
        from termsConditionsUriValue in termsConditionsUriToken.ToJValue()
        from termsConditionsUri in TermsConditionsUri.ValidTermsConditionsUri(termsConditionsUriValue)
        from hashToken in jObject.GetByKey("QC_hash")
        from hashValue in hashToken.ToJValue()
        let qesTransactionData = new QCertCreationTransactionData(transactionDataProperties, termsConditionsUri, hashValue.ToString(CultureInfo.InvariantCulture))
        select TransactionData.WithQCertCreationTransactionData(qesTransactionData);
}
