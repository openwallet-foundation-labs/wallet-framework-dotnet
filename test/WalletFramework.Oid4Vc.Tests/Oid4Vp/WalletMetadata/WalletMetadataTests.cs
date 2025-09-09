using FluentAssertions;
using Newtonsoft.Json.Linq;
using static WalletFramework.Oid4Vc.Oid4Vp.Models.WalletMetadata;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.WalletMetadata;

public class WalletMetadataTests
{
    [Fact]
    public void CreateDefault_ToJsonString_ShouldReturnValidJsonWithExpectedStructure()
    {
        // Arrange
        var expectedJson = @"{
            ""vp_formats_supported"": {
                ""vc+sd-jwt"": {
                    ""sd-jwt_alg_values"": [""ES256"", ""ES384"", ""ES512"", ""RS256""],
                    ""kb-jwt_alg_values"": [""ES256""]
                },
                ""dc+sd-jwt"": {
                    ""sd-jwt_alg_values"": [""ES256"", ""ES384"", ""ES512"", ""RS256""],
                    ""kb-jwt_alg_values"": [""ES256""]
                },
                ""mso_mdoc"": {
                    ""issuerauth_alg_values"": [""-7"", ""-35"", ""-36"", ""-8""],
                    ""deviceauth_alg_values"": [""-7""]
                }
            },
            ""client_id_prefixes_supported"": [""redirect_uri"", ""x509_san_dns""],
            ""client_id_schemes_supported"": [""redirect_uri"", ""x509_san_dns""]
        }";
        
        var expectedObject = JObject.Parse(expectedJson);
        
        // Act
        var actualJsonString = CreateDefault().ToJsonString();
        var actualObject = JObject.Parse(actualJsonString);
        
        // Assert
        actualObject.Should().BeEquivalentTo(expectedObject);
    }
}
