using Newtonsoft.Json.Linq;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci
{
    public static class Samples
    {
        public const string CredentialEndpoint = CredentialIssuer + "/credential";
        public const string CredentialIssuer = "https://test-issuer.de";

        public static string AuthorizationRequestJson =>
            new JObject
                {
                    ["response_uri"] = "https://lissi-test-verifier.de/authorization-response",
                    ["client_id_scheme"] = "x509_san_dns",
                    ["response_type"] = "vp_token",
                    ["presentation_definition"] = new JObject
                    {
                        ["id"] = "e325164b-5699-4deb-b3ee-e7b2d75e5034",
                        ["input_descriptors"] = new JArray
                        {
                            new JObject
                            {
                                ["id"] = "64863101-5049-407f-97e6-f6eb2deed16d",
                                ["format"] = new JObject
                                {
                                    ["vc+sd-jwt"] = new JObject()
                                },
                                ["constraints"] = new JObject
                                {
                                    ["fields"] = new JArray
                                    {
                                        new JObject
                                        {
                                            ["path"] = new JArray { "$.vct" },
                                            ["filter"] = new JObject
                                            {
                                                ["type"] = "string",
                                                ["const"] = "https://lissi-test.de/VerifiedEMail"
                                            }
                                        },
                                        new JObject
                                        {
                                            ["path"] = new JArray { "$.email" }
                                        }
                                    },
                                    ["limit_disclosure"] = "required"
                                }
                            }
                        }
                    },
                    ["state"] = "e325164b-5699-4deb-b3ee-e7b2d75e5034",
                    ["nonce"] = "kkxicbKxPSViBh97jqSB6r",
                    ["client_id"] = "lissi-test-verifier.de",
                    ["response_mode"] = "direct_post"
                }
                .ToString();

        public static string IssuerMetadataJson =>
            new JObject
                {
                    ["credential_issuer"] = CredentialIssuer,
                    ["credential_endpoint"] = CredentialEndpoint,
                    ["display"] = new JArray
                    {
                        new JObject
                        {
                            ["name"] = "Test Company GmbH",
                            ["logo"] = "https://test-issuer.com/logo.png",
                            ["locale"] = "en-US"
                        },
                        new JObject
                        {
                            ["name"] = "Test Company GmbH",
                            ["logo"] = "https://test-issuer.com/logo.png",
                            ["locale"] = "de-DE"
                        }
                    },
                    ["credentials_supported"] = new JObject
                    {
                        ["VerifiedEMailSdJwtVc"] = new JObject
                        {
                            ["format"] = "vc+sd-jwt",
                            ["scope"] = "VerifiedEMailSdJwtVc",
                            ["cryptographic_binding_methods_supported"] = new JArray { "jwk" },
                            ["cryptographic_suites_supported"] = new JArray { "ES256" },
                            ["display"] = new JArray
                            {
                                new JObject
                                {
                                    ["name"] = "Verified e-mail adress",
                                    ["logo"] = new JObject
                                    {
                                        ["url"] = "https:/test-issuer.com/credential-logo.png"
                                    },
                                    ["background_color"] = "#12107c",
                                    ["text_color"] = "#FFFFFF",
                                    ["locale"] = "en-US"
                                }
                            },
                            ["credential_definition"] = new JObject
                            {
                                ["vct"] = "https://test-issuer.com/VerifiedEMail",
                                ["claims"] = new JObject
                                {
                                    ["given_name"] = new JObject
                                    {
                                        ["display"] = new JArray
                                        {
                                            new JObject
                                            {
                                                ["locale"] = "de-DE",
                                                ["name"] = "Vorname"
                                            },
                                            new JObject
                                            {
                                                ["locale"] = "en-US",
                                                ["name"] = "Given name"
                                            }
                                        }
                                    },
                                    ["family_name"] = new JObject
                                    {
                                        ["display"] = new JArray
                                        {
                                            new JObject
                                            {
                                                ["locale"] = "de-DE",
                                                ["name"] = "Nachname"
                                            },
                                            new JObject
                                            {
                                                ["locale"] = "en-US",
                                                ["name"] = "Surname"
                                            }
                                        }
                                    },
                                    ["email"] = new JObject
                                    {
                                        ["display"] = new JArray
                                        {
                                            new JObject
                                            {
                                                ["locale"] = "de-DE",
                                                ["name"] = "E-Mail Adresse"
                                            },
                                            new JObject
                                            {
                                                ["locale"] = "en-US",
                                                ["name"] = "e-Mail address"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                .ToString();
    }
}
