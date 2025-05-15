using FluentAssertions;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Query;
using WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.Samples;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql;

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
                missing[0].GetIdentifier().Should().Be(query.CredentialQueries[0].Id.AsString());
            },
            () => Assert.Fail("Expected missing credentials, but got none.")
        );
    }
}
