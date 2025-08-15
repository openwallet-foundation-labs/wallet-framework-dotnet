using FluentAssertions;
using LanguageExt;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.AuthRequest;

public class ClientMetadataTests
{
    [Fact]
    public void IsVpFormatsSupported_WithDcqlQueryAndMissingVpFormats_ShouldReturnTrue()
    {
        // Arrange
        var clientMetadata = CreateClientMetadata();
        var dcqlQuery = new DcqlQuery
        {
            CredentialQueries =
            [
                new CredentialQuery
                {
                    Id = CredentialQueryId.Create("test-id").UnwrapOrThrow(),
                    Format = Constants.SdJwtDcFormat
                }
            ]
        };

        // Act
        var result = clientMetadata.VpFormatsSupportedValidation(dcqlQuery, Option<Uri>.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public void IsVpFormatsSupported_UsingSdJwt_WithSupportedVpFormats_ShouldSucceed()
    {
        // Arrange
        var clientMetadata = CreateClientMetadata() with
        {
            VpFormatsSupported = new Formats
            {
                SdJwtDcFormat = new SdJwtFormat
                {
                    IssuerSignedJwtAlgValues = ["ES123", "ES256", "ES384"],
                    KeyBindingJwtAlgValues = ["ES256"]
                }
            },
            VpFormats = new Formats
            {
                SdJwtDcFormat = new SdJwtFormat
                {
                    IssuerSignedJwtAlgValues = ["ES256"],
                    KeyBindingJwtAlgValues = ["ES256"]
                }
            }
        };

        var dcqlQuery = new DcqlQuery
        {
            CredentialQueries = new[]
            {
                new CredentialQuery
                {
                    Id = CredentialQueryId.Create("test-id").UnwrapOrThrow(),
                    Format = Constants.SdJwtDcFormat
                }
            }
        };

        // Act
        var result = clientMetadata.VpFormatsSupportedValidation(dcqlQuery, Option<Uri>.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public void IsVpFormatsSupported_UsingMDoc_WithSupportedVpFormats_ShouldSucceed()
    {
        // Arrange
        var clientMetadata = CreateClientMetadata() with
        {
            VpFormatsSupported = new Formats
            {
                MDocFormat = new MDocFormat()
                {
                    DeviceAuthAlgValues = ["-7"],
                    IssuerAuthAlgValues = ["-1", "-2", "-7"]
                }
            }
        };

        var dcqlQuery = new DcqlQuery
        {
            CredentialQueries = new[]
            {
                new CredentialQuery
                {
                    Id = CredentialQueryId.Create("test-id").UnwrapOrThrow(),
                    Format = Constants.MdocFormat
                }
            }
        };

        // Act
        var result = clientMetadata.VpFormatsSupportedValidation(dcqlQuery, Option<Uri>.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public void IsVpFormatsSupported_UsingSdJwtAndMDoc_WithSupportedVpFormats_ShouldSucceed()
    {
        // Arrange
        var clientMetadata = CreateClientMetadata() with
        {
            VpFormatsSupported = new Formats
            {
                MDocFormat = new MDocFormat()
                {
                    DeviceAuthAlgValues = ["-7"],
                    IssuerAuthAlgValues = ["-1", "-2", "-7"]
                },
                SdJwtDcFormat = new SdJwtFormat
                {
                    IssuerSignedJwtAlgValues = ["ES123", "ES256", "ES384"],
                    KeyBindingJwtAlgValues = ["ES256"]
                }
            }
        };

        var dcqlQuery = new DcqlQuery
        {
            CredentialQueries = new[]
            {
                new CredentialQuery
                {
                    Id = CredentialQueryId.Create("test-id-1").UnwrapOrThrow(),
                    Format = Constants.SdJwtDcFormat
                },
                new CredentialQuery
                {
                    Id = CredentialQueryId.Create("test-id-2").UnwrapOrThrow(),
                    Format = Constants.MdocFormat
                }
            }
        };

        // Act
        var result = clientMetadata.VpFormatsSupportedValidation(dcqlQuery, Option<Uri>.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public void IsVpFormatsSupported_UsingSdJwt_WithOnlyUnsupportedAlgorithms_ShouldFail()
    {
        // Arrange
        var clientMetadata = CreateClientMetadata() with
        { 
            VpFormatsSupported = new Formats
            {
                MDocFormat = new MDocFormat()
                {
                    DeviceAuthAlgValues = ["-6"],
                    IssuerAuthAlgValues = ["-1", "-2"]
                }
            }
        };

        var dcqlQuery = new DcqlQuery
        {
            CredentialQueries = new[]
            {
                new CredentialQuery
                {
                    Id = CredentialQueryId.Create("test-id").UnwrapOrThrow(),
                    Format = Constants.MdocFormat
                }
            }
        };

        // Act
        var result = clientMetadata.VpFormatsSupportedValidation(dcqlQuery, Option<Uri>.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
    
    [Fact]
    public void IsVpFormatsSupported_UsingMDoc_WithOnlyUnsupportedAlgorithms_ShouldFail()
    {
        // Arrange
        var clientMetadata = CreateClientMetadata() with
        { 
            VpFormatsSupported = new Formats
            {
                SdJwtDcFormat = new SdJwtFormat
                {
                    IssuerSignedJwtAlgValues = new List<string>() { "ES123" },
                    KeyBindingJwtAlgValues = new List<string>() { "ES123" }
                }
            }
        };

        var dcqlQuery = new DcqlQuery
        {
            CredentialQueries = new[]
            {
                new CredentialQuery
                {
                    Id = CredentialQueryId.Create("test-id").UnwrapOrThrow(),
                    Format = Constants.SdJwtDcFormat
                }
            }
        };

        // Act
        var result = clientMetadata.VpFormatsSupportedValidation(dcqlQuery, Option<Uri>.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    private static ClientMetadata CreateClientMetadata() => 
        new (
            authorizationEncryptedResponseEnc: null,
            encryptedResponseEncValuesSupported: null,
            redirectUris: new[] { "https://example.com" },
            clientName: "Test Client",
            clientUri: null,
            contacts: null,
            logoUri: null,
            jwks: LanguageExt.Option<WalletFramework.Oid4Vc.Oid4Vp.Jwk.JwkSet>.None,
            jwksUri: null,
            policyUri: null,
            tosUri: null,
            vpFormats: null,
            vpFormatsSupported: null);
}
