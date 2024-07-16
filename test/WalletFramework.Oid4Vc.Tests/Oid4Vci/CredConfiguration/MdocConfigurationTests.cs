using FluentAssertions;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json.Errors;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.Samples.Mdoc;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.MdocConfiguration;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci.CredConfiguration;

public class MdocConfigurationTests
{
    [Fact]
    public void Can_Parse()
    {
        // Arrange
        var sample = MdocConfigurationSample.Valid;

        // Act
        ValidMdocConfiguration(sample).Match(
            // Assert
            sut =>
            {
                sut.Format.Should().Be(MdocConfigurationSample.Format);
                sut.DocType.Should().Be(MdocConfigurationSample.DocType);
                sut.Policy.Match(
                    policy =>
                    {
                        policy.OneTimeUse.Should().Be(MdocConfigurationSample.OneTimeUse);
                        policy.BatchSize.Match(
                            batchSize => batchSize.Should().Be(MdocConfigurationSample.BatchSize),
                            () => Assert.Fail("BatchSize must be some")
                        );
                    },
                    () => Assert.Fail("Policy must be some"));
                
                sut.CryptographicSuitesSupported.Match(
                    list => list.Should().Contain(MdocConfigurationSample.CryptoSuite),
                    () => Assert.Fail("CryptographicSuitesSupported must be some"));
                
                sut.CryptographicCurvesSupported.Match(
                    list => list.Should().Contain(MdocConfigurationSample.CryptoCurve),
                    () => Assert.Fail("CryptographicCurvesSupported must be some"));
                
                sut.Claims.Match(
                    claims =>
                    {
                        var dict = claims.Value[MdocConfigurationSample.NameSpace];
                        dict[MdocConfigurationSample.GivenName].Display.Match(
                            list =>
                            {
                                list.Should().Contain(MdocConfigurationSample.EnglishDisplay);
                                list.Should().Contain(MdocConfigurationSample.JapaneseDisplay);
                            },
                            () => Assert.Fail("Display must be some"));
                    },
                    () => Assert.Fail("Claims must be some"));
            },
            _ => Assert.Fail("Must be valid")
        );
    }

    [Fact]
    public void Configuration_With_Invalid_Structure_Is_Rejected()
    {
        // Arrange
        var sample = MdocConfigurationSample.Valid;

        sample["format"] = "";
        sample["doctype"] = null;

        ValidMdocConfiguration(sample).Match(
            _ => Assert.Fail("MdocConfiguration must be invalid"),
            errors =>
            {
                errors.Count.Should().Be(2);
                errors.Should().AllBeOfType<JsonFieldValueIsNullOrWhitespaceError>();
            });
    }
}
