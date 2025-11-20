using FluentAssertions;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.CredConfiguration.Mdoc.Samples;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.CredConfiguration.SdJwt.Samples;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.Issuer.Samples;
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
}
