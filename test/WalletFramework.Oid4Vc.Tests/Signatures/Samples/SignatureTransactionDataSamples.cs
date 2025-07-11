using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Tests.Signatures.Samples;

public static class SignatureTransactionDataSamples
{
    public static string CscSignatureJsonSample => new JObject
    {
        ["type"] = "https://cloudsignatureconsortium.org/2025/qes",
        ["credential_ids"] = new JArray
        {
            "my_credential"
        },
        ["numSignatures"] = 1,
        ["signatureQualifier"] = "eu_eidas_qes",
        ["documentDigests"] = new JArray
        {
            new JObject
            {
                ["label"] = "Example Contract",
                ["hashType"] = "sodr",
                ["hash"] = "HZQzZmMAIWekfGH0/ZKW1nsdt0xg3H6bZYztgsMTLw0="
            }
        },
        ["processID"] = "eOZ6UwXyeFLK98Do51x33fmuv4OqAz5Zc4lshKNtEgQ=",
        ["transaction_data_hashes_alg"] = "sha-256"
    }.ToString();

    public static Base64UrlString ToBase64UrlString()
    {
        var encoded = Base64UrlEncoder.Encode(CscSignatureJsonSample);
        return Base64UrlString.FromString(encoded).UnwrapOrThrow();
    }
}
