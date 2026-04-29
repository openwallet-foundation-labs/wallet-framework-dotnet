using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vp.Tests.TS12SCA.Samples;

public static class Ts12PaymentTransactionDataSamples
{
    private const string AuthRequestWithTransactionDataTemplate = @"{
  ""response_uri"": ""https://test.test.test.io/openid4vp/authorization-response"",
  ""transaction_data"": [
    ""{0}""
  ],
  ""client_id_scheme"": ""x509_san_dns"",
  ""iss"": ""https://test.test.test.io"",
  ""response_type"": ""vp_token"",
  ""nonce"": ""bRlAPdfKK2rSyn8RKoYDkr"",
  ""client_id"": ""test.test.test.io"",
  ""response_mode"": ""direct_post"",
  ""aud"": ""https://self-issued.me/v2"",
  ""dcql_query"": {1},
  ""state"": ""73ec8b46-2289-4a31-856c-06ef56cdf165"",
  ""exp"": 1747316703,
  ""iat"": 1747313103,
  ""client_metadata"": {
    ""client_name"": ""default-test-updated"",
    ""logo_uri"": ""https://www.defaultTestLogo.com/updated-logo.png"",
    ""redirect_uris"": [""https://test.id""],
    ""tos_uri"": ""https://www.example.com/tos"",
    ""policy_uri"": ""https://www.example.com/policy"",
    ""client_uri"": ""https://www.example.com"",
    ""contacts"": [""admin@admin.it""],
    ""vp_formats"": {
      ""vc+sd-jwt"": {
        ""sd-jwt_alg_values"": [""ES256""],
        ""kb-jwt_alg_values"": [""ES256""]
      },
      ""dc+sd-jwt"": {
        ""sd-jwt_alg_values"": [""ES256""],
        ""kb-jwt_alg_values"": [""ES256""]
      }
    }
  }
}";

    public static string JsonSample => new JObject
    {
        ["type"] = "urn:eudi:sca:payment:1",
        ["credential_ids"] = new JArray
        {
            "idcard"
        },
        ["transaction_data_hashes_alg"] = new JArray
        {
            "sha-256"
        },
        ["payload"] = new JObject
        {
            ["transaction_id"] = "ts12-transaction-123",
            ["date_time"] = "2026-04-29T12:30:00Z",
            ["payee"] = new JObject
            {
                ["name"] = "Merchant XYZ",
                ["id"] = "merchant-xyz",
                ["logo"] = "https://merchant.example/logo.png",
                ["website"] = "https://merchant.example"
            },
            ["pisp"] = new JObject
            {
                ["legal_name"] = "Payment Initiator Ltd",
                ["brand_name"] = "PayFast",
                ["domain_name"] = "payfast.example"
            },
            ["execution_date"] = "2026-05-01",
            ["currency"] = "EUR",
            ["amount"] = 23.58m,
            ["amount_estimated"] = true,
            ["amount_earmarked"] = false,
            ["sct_inst"] = true,
            ["recurrence"] = new JObject
            {
                ["start_date"] = "2026-05-01",
                ["end_date"] = "2026-12-01",
                ["number"] = 8,
                ["frequency"] = "MNTH",
                ["mit_options"] = new JObject
                {
                    ["amount_variable"] = true,
                    ["min_amount"] = 10.01m,
                    ["max_amount"] = 100.99m,
                    ["total_amount"] = 800.50m,
                    ["initial_amount"] = 5.25m,
                    ["initial_amount_number"] = 2,
                    ["apr"] = 3.14m
                }
            }
        }
    }.ToString();

    public static Base64UrlString GetBase64UrlStringSample()
    {
        return GetBase64UrlString(JsonSample);
    }

    public static Base64UrlString GetBase64UrlString(string json)
    {
        var encoded = Base64UrlEncoder.Encode(json);
        return Base64UrlString.FromString(encoded).UnwrapOrThrow();
    }

    public static string GetAuthRequestWithTs12PaymentTransactionDataStr(string dcqlQueryJson)
    {
        var encodedTransactionData = GetBase64UrlStringSample().AsString;
        return AuthRequestWithTransactionDataTemplate
            .Replace("{0}", encodedTransactionData)
            .Replace("{1}", dcqlQueryJson);
    }
}
