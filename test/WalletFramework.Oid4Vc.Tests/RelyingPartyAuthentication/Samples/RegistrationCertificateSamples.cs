using System.Globalization;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Tests.RelyingPartyAuthentication.Samples;

public static class RegistrationCertificateSamples
{
    public static JObject JsonSample => new JObject
    {
        ["purpose"] = new JArray
        {
            new JObject
            {
                ["locale"] = "en-US",
                ["name"] = "Onboarding of a new user to the system.",
            }
        },
        ["contact"] = new JObject
        {
            ["address"] = "Lagerhofstr. 4, 04103 Leipzig, Germany",
            ["email"] = "privacy@sprind.org",
            ["phone"] = "1234566789",
        },
        ["sub"] = "CN=SPRIND GmbH, C=DE",
        ["privacy_policy"] = "https://sprind.org/en/privacy-policy",
        ["iat"] = 1683000000,
        ["credentials"] = new JArray
        {
            new JObject
            {
                ["id"] = "pid",
                ["format"] = "dc+sd-jwt",
                ["meta"] = new JObject
                {
                    ["vct_values"] = new JArray
                    {
                        "https://credentials.example.com/identity_credential"
                    }
                },
                ["claims"] = new JArray
                {
                    new JObject
                    {
                        ["id"] = "a", 
                        ["path"] = new JArray
                        {
                            new JValue ("given_name")
                        },
                        ["purpose"] = new JArray
                        {
                            new JObject
                            {
                                ["locale"] = "en-US",
                                ["name"] = "required for identification",
                            }   
                        }
                    },
                    new JObject
                    {
                        ["id"] = "b", 
                        ["path"] = new JArray
                        {
                            new JValue ("family_name")
                        },
                        ["purpose"] = new JArray
                        {
                            new JObject
                            {
                                ["locale"] = "en-US",
                                ["name"] = "required for identification",
                            }   
                        }
                    },
                },
                ["claim_sets"] = new JArray
                {
                    new JArray
                    {
                        new JValue ("a"),
                        new JValue ("b"),
                    }
                }
            }
        },
        ["credential_sets"] = new JArray
        {
            new JObject
            {
                ["purpose"] = new JArray
                {
                    new JObject
                    {
                        ["locale"] = "en-US",
                        ["name"] = "required for identification",
                    }   
                },
                ["options"] = new JArray
                {
                    new JArray
                    {
                        new JValue("pid")
                    },
                    new JArray
                    {
                        new JValue("other_id")
                    },
                    new JArray
                    {
                        new JValue("pid_reduced_cred_1"),
                        new JValue("pid_reduced_cred_2")
                    },
                }
            }
        },
        ["status"] = new JObject
        {
            ["status_list"] = new JObject
            {
                ["idx"] = 0,
                ["uri"] = "https://example.com/statuslists/1"
            }
        }
    };
}
