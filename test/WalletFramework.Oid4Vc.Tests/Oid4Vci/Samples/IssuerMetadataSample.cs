using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Uri;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.Samples.Mdoc;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.Samples.SdJwt;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci.Samples;

public static class IssuerMetadataSample
{
    public static Uri CredentialEndpoint => new(CredentialIssuer + "/credential");
    
    public static Uri CredentialIssuer => new("https://test-issuer.de");

    public static CredentialConfigurationId MdocConfigurationId => CredentialConfigurationId
        .ValidCredentialConfigurationId(MdocConfigurationSample.DocType.ToString())
        .UnwrapOrThrow(new InvalidOperationException());
    
    public static CredentialConfigurationId SdJwtConfigurationId => CredentialConfigurationId
        .ValidCredentialConfigurationId(SdJwtConfigurationSample.Scope.ToString())
        .UnwrapOrThrow(new InvalidOperationException());

    public static JObject EncodedAsJson => new()
    {
        ["credential_issuer"] = CredentialIssuer.ToStringWithoutTrail(),
        ["credential_endpoint"] = CredentialEndpoint.ToStringWithoutTrail(),
        ["display"] = new JArray
        {
            new JObject
            {
                ["name"] = "Test Company GmbH",
                ["logo"] = new JObject
                {
                    { "uri", "https://test-issuer.com/logo.png" }
                },
                ["locale"] = "en-US"
            },
            new JObject
            {
                ["name"] = "Test Company GmbH",
                ["logo"] = new JObject
                {
                    { "uri", "https://test-issuer.com/logo.png" }
                },
                ["locale"] = "de-DE"
            }
        },
        ["authorization_servers"] = new JArray { "https://test-issuer.com/authorizationserver"  },
        ["credential_configurations_supported"] = new JObject
        {
            [SdJwtConfigurationId] = SdJwtConfigurationSample.Valid,
            [MdocConfigurationId] = MdocConfigurationSample.Valid
        }
    };
    
    public static IssuerMetadata Decoded => 
        IssuerMetadata.ValidIssuerMetadata(EncodedAsJson).UnwrapOrThrow(new InvalidOperationException());
}
