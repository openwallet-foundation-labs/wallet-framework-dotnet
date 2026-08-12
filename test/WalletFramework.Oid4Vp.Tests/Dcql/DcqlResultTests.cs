using FluentAssertions;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Oid4Vp.Dcql;
using WalletFramework.Oid4Vp.Models;
using WalletFramework.Oid4Vp.Tests.Dcql.Samples;

namespace WalletFramework.Oid4Vp.Tests.Dcql;

public class DcqlResultTests
{
    [Fact]
    public void Result_Can_Show_Missing_Credentials()
    {
        // Arrange
        var query = DcqlSamples.GetNoMatchErrorClaimPathQuery();
        var credentials = Array.Empty<ICredential>();

        // Act
        var result = query.ProcessWith(credentials);

        // Assert
        result.FlattenCandidates().IsNone.Should().BeTrue();
        result.MissingCredentials.Match(
            missing =>
            {
                missing.Should().HaveCount(1);
                missing[0].Id.AsString().Should().Be(query.CredentialQueries[0].Id.AsString());
            },
            () => Assert.Fail("Expected missing credentials, but got none.")
        );
    }
}
