using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Tests.Payment.Samples;

public static class PaymentTransactionDataSamples
{
    public static string JsonSample => new JObject
    {
        ["type"] = "payment_data",
        ["credential_ids"] = new JArray
        {
            "credential-id-1",
            "credential-id-2"
        },
        ["payment_data"] = new JObject
        {
            ["payee"] = "Merchant XYZ",
            ["currency_amount"] = new JObject
            {
                ["currency"] = "EUR",
                ["value"] = "23.58"
            },
            ["recurring_schedule"] = new JObject
            {
                ["start_date"] = "2024-11-01",
                ["expiry_date"] = "2025-10-31",
                ["frequency"] = 30
            }
        }
    }.ToString();

    public static Base64UrlString GetBase64UrlStringSample()
    {
        var str = JsonSample;
        var encoded = Base64UrlEncoder.Encode(str);
        return Base64UrlString.FromString(encoded).UnwrapOrThrow();
    }
}
