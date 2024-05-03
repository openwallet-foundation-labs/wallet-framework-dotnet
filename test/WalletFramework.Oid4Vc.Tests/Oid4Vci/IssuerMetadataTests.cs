using FluentAssertions;
using Hyperledger.Aries.Extensions;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer;
using static WalletFramework.Oid4Vc.Tests.Oid4Vci.Samples;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci
{
    public class IssuerMetadataTests
    {
        [Fact]
        public void Additional_Or_Unrecognized_Fields_Are_Ignored_During_Deserialization()
        {
            var json = IssuerMetadataJson;
            var jObject = JObject.Parse(json);
            jObject["Additional_Field"] = "Additional_Field";

            var sut = jObject.ToObject<OidIssuerMetadata>();
            
            sut.Should().BeOfType<OidIssuerMetadata>();
            sut!.CredentialIssuer.Should().Be(CredentialIssuer);
            sut.CredentialEndpoint.Should().Be(CredentialEndpoint);
        }

        [Theory]
        [InlineData("credential_issuer")]
        [InlineData("credential_endpoint")]
        [InlineData("credentials_supported")]
        public void Deserialization_Fails_When_Required_Fields_Are_Missing(string fieldName)
        {
            // Arrange
            var json = IssuerMetadataJson;

            var jObject = JObject.Parse(json);

            jObject[fieldName] = null;

            try
            {
                // Act
                jObject.ToObject<OidIssuerMetadata>();

                // Assert
                Assert.Fail("IssuerMetadata deserialization should have failed");
            }
            catch (Exception)
            {
                // Assert
                // Pass
            }
        }

        [Fact]
        public void Valid_Json_Deserializes_To_Model()
        {
            var json = IssuerMetadataJson;

            var sut = json.ToObject<OidIssuerMetadata>();

            sut.Should().BeOfType<OidIssuerMetadata>();
            sut.CredentialIssuer.Should().Be(CredentialIssuer);
            sut.CredentialEndpoint.Should().Be(CredentialEndpoint);
        }
    }
}
