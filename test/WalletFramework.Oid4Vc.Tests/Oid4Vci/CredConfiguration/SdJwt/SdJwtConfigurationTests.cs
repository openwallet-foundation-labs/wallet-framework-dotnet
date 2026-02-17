using FluentAssertions;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.CredConfiguration.SdJwt.Samples;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.SdJwt.SdJwtConfiguration;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci.CredConfiguration.SdJwt;

public class SdJwtConfigurationTests
{
    [Fact]
    public void Can_Parse()
    {
        // Arrange
        var sample = SdJwtConfigurationSample.ValidSample;

        // Act
        ValidSdJwtCredentialConfiguration(sample).Match(
            // Assert
            sut =>
            {
                sut.Format.Should().Be(SdJwtConfigurationSample.Format);
                sut.Vct.Should().Be(SdJwtConfigurationSample.Vct);

                sut.CredentialConfiguration.Scope.Match(
                    scope => scope.Should().Be(SdJwtConfigurationSample.Scope),
                    () => Assert.Fail("Scope must be some"));

                sut.CredentialConfiguration.CryptographicBindingMethodsSupported.Match(
                    methods => methods.Should().ContainSingle(method => method.ToString() == "jwk"),
                    () => Assert.Fail("CryptographicBindingMethodsSupported must be some"));

                sut.CredentialConfiguration.CredentialSigningAlgValuesSupported.Match(
                    values => values.Should().ContainSingle(value => value.ToString() == "ES256"),
                    () => Assert.Fail("CredentialSigningAlgValuesSupported must be some"));

                sut.CredentialConfiguration.ProofTypesSupported.Match(
                    proofTypes =>
                    {
                        proofTypes.Keys.Should().ContainSingle(key => key.ToString() == "jwt");
                        proofTypes.Values.Single().ProofSigningAlgValuesSupported
                            .Should()
                            .ContainSingle(value => value.ToString() == "ES256");
                    },
                    () => Assert.Fail("ProofTypesSupported must be some"));

                sut.CredentialConfiguration.CredentialMetadata.Match(
                    metadata =>
                    {
                        metadata.Display.Match(
                            displays => displays.Should().HaveCount(1),
                            () => Assert.Fail("Display must be some"));

                        metadata.Claims.Match(
                            claims =>
                            {
                                claims.Should().HaveCount(9);
                                claims.Select(claim => claim.Path.ToJsonPath().ToString())
                                    .Should()
                                    .Contain("$.degrees[*].type");
                            },
                            () => Assert.Fail("Claims must be some"));
                    },
                    () => Assert.Fail("CredentialMetadata must be some"));
            },
            _ => Assert.Fail("Must be valid")
        );
    }
}
