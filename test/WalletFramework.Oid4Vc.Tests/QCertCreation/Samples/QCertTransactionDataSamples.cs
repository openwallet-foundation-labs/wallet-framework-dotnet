using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Tests.QCertCreation.Samples;

public static class QCertCreationTransactionDataSamples
{
    public static string QCertJsonSample => new JObject
    {
        ["type"] = "qcert_creation_acceptance",
        ["QC_terms_conditions_uri"] = "https://example.com/tos",
        ["QC_hash"] = "kXAgwDcdAe3obxpo8UoDkC-D-b7OCrDo8IOGZjSX8_M=",
        ["QC_hashAlgorithmOID"] = "2.16.840.1.101.3.4.2.1",
        ["credential_ids"] = new JArray
        {
            "credential-id-1",
            "credential-id-2"
        },
    }.ToString();

    public static string CscQCertJsonSample => new JObject
    {
        ["type"] = "https://cloudsignatureconsortium.org/2025/qc-request",
        ["credential_ids"] = new JArray
        {
            "credential-id-1"
        },
        ["QC_terms_conditions_uri"] = "https://qtsp.example.com/policies/terms_and_conditions.pdf",
        ["QC_hash"] = "ohxKcClPp/J1dI1iv5x519BpjduGZC794x4ABFeb+Ds=",
        ["QC_hashAlgorithmOID"] = "2.16.840.1.101.3.4.2.1",
        ["transaction_data_hashes_alg"] = "sha-256"
    }.ToString();

    public static Base64UrlString ToBase64UrlString(string qCertJsonString)
    {
        var encoded = Base64UrlEncoder.Encode(qCertJsonString);
        return Base64UrlString.FromString(encoded).UnwrapOrThrow();
    }
}
