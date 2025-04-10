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
    public void Can_Decode_Draft14_From_Json()
    {
        // Arrange
        var sample = IssuerMetadataSample.EncodedAsJsonDraft14AndLower;
    
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
        var issuerMetadata = IssuerMetadataSample.DecodedDraft14AndLower;

        var sut = issuerMetadata.EncodeToJson();

        sut.Should().BeEquivalentTo(IssuerMetadataSample.EncodedAsJsonDraft14AndLower);
    }

    [Fact]
    public void Can_Decode_And_Encode_From_Json_Draft14()
    {
        // Arrange
        var sample = IssuerMetadataSample.EncodedAsJsonDraft14AndLower;

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
    
    [Fact]
    public void Can_Decode_And_Encode_From_Json_Draft15()
    {
        // Arrange
        var sampleDraft15 = IssuerMetadataSample.EncodedAsJsonDraft15;
        var sampleDraft14 = IssuerMetadataSample.EncodedAsJsonDraft14AndLower;
        
        // Act
        ValidIssuerMetadata(sampleDraft15).Match(
            // Assert
            issuerMetadata =>
            {
                var sut = issuerMetadata.EncodeToJson();
                sut.Should().BeEquivalentTo(sampleDraft14);
            },
            _ => Assert.Fail("IssuerMetadata must be valid"));
    }
}
