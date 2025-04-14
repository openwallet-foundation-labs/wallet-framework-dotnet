using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;
using WalletFramework.SdJwtVc.Models;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci.CredConfiguration.SdJwt.Samples;

public static class SdJwtConfigurationSample
{
    public static Format Format => Format
        .ValidFormat(Constants.SdJwtDcFormat)
        .UnwrapOrThrow(new InvalidOperationException());
    
    public static Vct Vct => Vct
        .ValidVct("https://test-issuer.com/VerifiedEMail")
        .UnwrapOrThrow(new InvalidOperationException());
    
    public static Scope Scope => Scope
        .OptionalScope("VerifiedEMailSdJwtVc")
        .ToNullable() ?? throw new InvalidOperationException();
    
    public static JObject ValidDraft15 => new()
    {
        ["format"] = Format.ToString(),
        ["scope"] = Scope.ToString(),
        ["cryptographic_binding_methods_supported"] = new JArray { "jwk" },
        ["credential_signing_alg_values_supported"] = new JArray { "ES256" },
        ["display"] = new JArray
        {
            new JObject
            {
                ["name"] = "Verified e-mail adress",
                ["logo"] = new JObject
                {
                    ["uri"] = "https://test-issuer.com/credential-logo.png"
                },
                ["background_color"] = "#12107c",
                ["text_color"] = "#FFFFFF",
                ["locale"] = "en-US"
            }
        },
        ["vct"] = Vct.ToString(),
        ["claims"] = new JArray()
        {
            new JObject
            {
                ["path"] = new JArray(){"given_name"},
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
            new JObject
            {
                ["path"] = new JArray(){"family_name"},
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
            new JObject
            {
                ["path"] = new JArray(){"email"},
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
            },
            new JObject
            {
                ["path"] = new JArray(){"address" },
                ["display"] = new JArray
                {
                    new JObject
                    {
                        ["locale"] = "de-DE",
                        ["name"] = "Adresse"
                    },
                    new JObject
                    {
                        ["locale"] = "en-US",
                        ["name"] = "Address"
                    }
                }
            },
            new JObject
            {
                ["path"] = new JArray(){"address", "street"},
                ["display"] = new JArray
                {
                    new JObject
                    {
                        ["locale"] = "de-DE",
                        ["name"] = "Straße"
                    },
                    new JObject
                    {
                        ["locale"] = "en-US",
                        ["name"] = "Street"
                    }
                }
            },
            new JObject
            {
                ["path"] = new JArray(){"address", "zip"},
                ["display"] = new JArray
                {
                    new JObject
                    {
                        ["locale"] = "de-DE",
                        ["name"] = "Postleitzahl"
                    },
                    new JObject
                    {
                        ["locale"] = "en-US",
                        ["name"] = "Zip Code"
                    }
                }
            },
            new JObject
            {
                ["path"] = new JArray(){"address", "zip", "building"},
                ["display"] = new JArray
                {
                    new JObject
                    {
                        ["locale"] = "de-DE",
                        ["name"] = "Gebäude"
                    },
                    new JObject
                    {
                        ["locale"] = "en-US",
                        ["name"] = "Building"
                    }
                }
            }
        }
    };
    public static JObject ValidDraft14AndLower => new()
    {
        ["format"] = Format.ToString(),
        ["scope"] = Scope.ToString(),
        ["cryptographic_binding_methods_supported"] = new JArray { "jwk" },
        ["credential_signing_alg_values_supported"] = new JArray { "ES256" },
        ["display"] = new JArray
        {
            new JObject
            {
                ["name"] = "Verified e-mail adress",
                ["logo"] = new JObject
                {
                    ["uri"] = "https://test-issuer.com/credential-logo.png"
                },
                ["background_color"] = "#12107c",
                ["text_color"] = "#FFFFFF",
                ["locale"] = "en-US"
            }
        },
        ["vct"] = Vct.ToString(),
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
            },
            ["address"] = new JObject
            {
                ["display"] = new JArray
                {
                    new JObject
                    {
                        ["locale"] = "de-DE",
                        ["name"] = "Adresse"
                    },
                    new JObject
                    {
                        ["locale"] = "en-US",
                        ["name"] = "Address"
                    }
                },
                ["street"] = new JObject
                {
                    ["display"] = new JArray
                    {
                        new JObject
                        {
                            ["locale"] = "de-DE",
                            ["name"] = "Straße"
                        },
                        new JObject
                        {
                            ["locale"] = "en-US",
                            ["name"] = "Street"
                        }
                    }
                },
                ["zip"] = new JObject
                {
                    ["display"] = new JArray
                    {
                        new JObject
                        {
                            ["locale"] = "de-DE",
                            ["name"] = "Postleitzahl"
                        },
                        new JObject
                        {
                            ["locale"] = "en-US",
                            ["name"] = "Zip Code"
                        }
                    },
                    ["building"] = new JObject
                    {
                        ["display"] = new JArray
                        {
                            new JObject
                            {
                                ["locale"] = "de-DE",
                                ["name"] = "Gebäude"
                            },
                            new JObject
                            {
                                ["locale"] = "en-US",
                                ["name"] = "Building"
                            }
                        }
                    }
                }
            }
        }
    };
}
