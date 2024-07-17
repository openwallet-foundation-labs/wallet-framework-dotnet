using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Uri;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.Samples;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.Samples.Mdoc;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.Samples.SdJwt;
using static WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models.IssuerMetadata;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci.Issuer;

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
        var decoded = IssuerMetadataSample.Decoded;

        var sut = JObject.FromObject(decoded).RemoveNulls().ToObject<JObject>();

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
            sut =>
            {
                var encoded = JObject.FromObject(sut).RemoveNulls().ToObject<JObject>();
                encoded.Should().BeEquivalentTo(sample);
            },
            _ => Assert.Fail("IssuerMetadata must be valid"));
    }

    [Fact]
    public void Can_Decode_From_Persisted_Json()
    {
        var sample = IssuerMetadataSample.EncodedAsJson;

        var sut = JsonConvert.DeserializeObject<IssuerMetadata>(sample.ToString())!;

        sut.CredentialIssuer.ToString().Should().Be(IssuerMetadataSample.CredentialIssuer.ToStringWithoutTrail());
    }
}
