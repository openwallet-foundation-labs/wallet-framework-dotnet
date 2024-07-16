using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;
using WalletFramework.SdJwtVc.Models;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci.Samples.SdJwt;

public static class SdJwtConfigurationSample
{
    public static Format Format => Format
        .ValidFormat("vc+sd-jwt")
        .UnwrapOrThrow(new InvalidOperationException());
    
    public static Vct Vct => Vct
        .ValidVct("https://test-issuer.com/VerifiedEMail")
        .UnwrapOrThrow(new InvalidOperationException());
    
    public static Scope Scope => Scope
        .OptionalScope("VerifiedEMailSdJwtVc")
        .ToNullable() ?? throw new InvalidOperationException();
    
    public static JObject Valid => new()
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
            }
        }
    };
}
