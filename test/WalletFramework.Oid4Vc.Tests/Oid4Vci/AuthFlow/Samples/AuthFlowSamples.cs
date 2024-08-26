using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.Issuer.Samples;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci.AuthFlow.Samples;

public static class AuthFlowSamples
{
    public static JObject AuthFlowSessionRecordJson => new()
    {
        ["authorization_data"] = new JObject
        {
            ["credential_oauth_token"] = new JObject
            {
                ["access_token"] = "i can write anything"
            },
            ["client_options"] = new JObject
            {
                ["ClientId"] = "https://test-issuer.com/redirect",
                ["WalletIssuer"] = "i can write anything",
                ["RedirectUri"] = "https://test-issuer.com/redirect"
            },
            ["issuer_metadata"] = IssuerMetadataSample.EncodedAsJson,
            ["authorization_server_metadata"] = new JObject
            {
                ["issuer"] = "i can write anything",
                ["token_endpoint"] = "i can write anything",
                ["jwks_uri"] = "i can write anything",
                ["authorization_endpoint"] = "i can write anything",
                ["response_types_supported"] = new JArray("i can write anything"),
            },
            ["credential_configuration_ids"] = new JArray("org.iso.18013.5.1.mDL")
        },
        ["authorization_code_parameters"] = new JObject
        {
            ["Challenge"] = "hello",
            ["CodeChallengeMethod"] = "S256",
            ["Verifier"] = "world"
        },
        ["RecordVersion"] = 1,
        ["Id"] = "598e7661-95a8-4531-b707-3d256d3c1745"
    };
}
