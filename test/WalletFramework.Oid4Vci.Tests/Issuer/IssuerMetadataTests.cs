using FluentAssertions;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vci.Issuer.Models;
using WalletFramework.Oid4Vci.Tests.CredConfiguration.Mdoc.Samples;
using WalletFramework.Oid4Vci.Tests.CredConfiguration.SdJwt.Samples;
using WalletFramework.Oid4Vci.Tests.Issuer.Samples;
using Xunit;
using static WalletFramework.Oid4Vci.Issuer.Models.IssuerMetadata;
using static WalletFramework.Oid4Vci.Issuer.Models.IssuerMetadataJsonExtensions;

namespace WalletFramework.Oid4Vci.Tests.Issuer;

public class IssuerMetadataTests
{
    [Fact]
    public void Can_Decode_From_Json()
    {
        // Arrange
        var sample = IssuerMetadataSample.EncodedAsJson;
    
        // Act
        ValidIssuerMetadata(sample).Match(
            // Assert
            sut =>
            {
                new Uri(sut.CredentialIssuer.ToString()).Should().Be(IssuerMetadataSample.CredentialIssuer);
                new Uri(sut.CredentialEndpoint.ToString()).Should().Be(IssuerMetadataSample.CredentialEndpoint);
    
                var mdocConfiguration = sut
                    .CredentialConfigurationsSupported[IssuerMetadataSample.MdocConfigurationId]
                    .AsT1;
    
                mdocConfiguration.Format.Should().Be(MdocConfigurationSample.Format);
                mdocConfiguration.DocType.Should().Be(MdocConfigurationSample.DocType);
                
                var sdJwtConfiguration = sut
                    .CredentialConfigurationsSupported[IssuerMetadataSample.SdJwtConfigurationId]
                    .AsT0;
                
                sdJwtConfiguration.Format.Should().Be(SdJwtConfigurationSample.Format);
                sdJwtConfiguration.Vct.Should().Be(SdJwtConfigurationSample.Vct);
            },
            _ => Assert.Fail("IssuerMetadata must be valid"));
    }

    [Fact]
    public void Can_Encode_To_Json()
    {
        var issuerMetadata = IssuerMetadataSample.Decoded;

        var sut = issuerMetadata.EncodeToJson();

        sut.Should().BeEquivalentTo(IssuerMetadataSample.EncodedAsJson);
    }

    [Fact]
    public void Can_Decode_And_Encode_From_Json()
    {
        // Arrange
        var sample = IssuerMetadataSample.EncodedAsJson;

        // Act
        ValidIssuerMetadata(sample).Match(
            // Assert
            issuerMetadata =>
            {
                var sut = issuerMetadata.EncodeToJson();
                sut.Should().BeEquivalentTo(sample);
            },
            _ => Assert.Fail("IssuerMetadata must be valid"));
    }

    [Fact(DisplayName = "issuer metadata preserves trailing slashes in issuer identifiers")]
    public void IssuerMetadataPreservesTrailingSlashesInIssuerIdentifiers()
    {
        const string credentialIssuer = "https://test-issuer.de/test/a/sdjwtEncrypted/";
        const string authorizationServer = "https://test-issuer.com/authorizationserver/";
        var sample = (JObject)IssuerMetadataSample.EncodedAsJson.DeepClone();
        sample[CredentialIssuerJsonKey] = credentialIssuer;
        sample[AuthorizationServersJsonKey] = new JArray(authorizationServer);

        ValidIssuerMetadata(sample).Match(
            issuerMetadata =>
            {
                var encoded = issuerMetadata.EncodeToJson();

                issuerMetadata.CredentialIssuer.ToString().Should().Be(credentialIssuer);
                encoded[CredentialIssuerJsonKey]!.Value<string>().Should().Be(credentialIssuer);
                encoded[AuthorizationServersJsonKey]!.Single()!.Value<string>().Should().Be(authorizationServer);
            },
            _ => Assert.Fail("IssuerMetadata must be valid"));
    }
}
