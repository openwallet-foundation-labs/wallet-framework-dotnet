using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.Samples;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci.AuthFlow.Samples;

public static class AuthFlowSamples
{
    public static JObject AuthFlowSessionRecordJson => new()
    {
        ["AuthorizationData"] = new JObject
        {
            ["ClientOptions"] = new JObject
            {
                ["ClientId"] = "https://test-issuer.com/redirect",
                ["WalletIssuer"] = "i can write anything",
                ["RedirectUri"] = "https://test-issuer.com/redirect"
            },
            ["IssuerMetadata"] = IssuerMetadataSample.EncodedAsJson,
            ["AuthorizationServerMetadata"] = new JObject
            {
                ["issuer"] = "i can write anything",
                ["token_endpoint"] = "i can write anything",
                ["jwks_uri"] = "i can write anything",
                ["authorization_endpoint"] = "i can write anything",
                ["response_types_supported"] = new JArray("i can write anything"),
            },
            ["CredentialConfigurationIds"] = new JArray("org.iso.18013.5.1.mDL")
        },
        ["AuthorizationCodeParameters"] = new JObject
        {
            ["Challenge"] = "hello",
            ["CodeChallengeMethod"] = "S256",
            ["Verifier"] = "world"
        },
        ["RecordVersion"] = 1,
        ["Id"] = "598e7661-95a8-4531-b707-3d256d3c1745"
    };
}
