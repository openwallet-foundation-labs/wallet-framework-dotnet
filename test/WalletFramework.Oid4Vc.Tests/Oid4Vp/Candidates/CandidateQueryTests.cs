using FluentAssertions;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Query;
using WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.Samples;
using WalletFramework.Oid4Vc.Tests.Samples;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Candidates;

public class CandidateQueryTests
{
    [Fact]
    public void Candidates_Can_Be_Built_From_Dcql_Query()
    {
        // Arrange
        var query = DcqlSamples.GetMdocAndSdJwtFamilyNameQuery();
        ICredential mdoc = MdocSamples.MdocRecord;
        ICredential sdJwt = SdJwtSamples.GetIdCardCredential();
        var credentials = new[] { mdoc, sdJwt };

        // Act
        var result = CandidateQueryResult.FromDcqlQuery(query, credentials);

        // Assert
        result.Candidates.Match(
            candidates =>
            {
                candidates.Should().HaveCount(2);
                var identifiers = candidates.Select(c => c.Identifier).ToList();
                identifiers.Should().Contain(query.CredentialQueries.Select(q => q.Id.AsString()));
            },
            () => Assert.Fail("Expected candidates, but got none.")
        );
        result.MissingCredentials.IsNone.Should().BeTrue();
    }

    [Fact]
    public void Result_Can_Show_Missing_Credentials()
    {
        // Arrange
        var query = DcqlSamples.GetNoMatchErrorClaimPathQuery();
        var credentials = Array.Empty<ICredential>();

        // Act
        var result = CandidateQueryResult.FromDcqlQuery(query, credentials);

        // Assert
        result.Candidates.IsNone.Should().BeTrue();
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
