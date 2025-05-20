using FluentAssertions;
using WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.Samples;
using WalletFramework.Oid4Vc.Tests.Samples;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql;

public class DcqlClaimSetsTests
{
    [Fact]
    public void The_First_Matching_Claim_Set_Is_Disclosed()
    {
        // Arrange
        var dcqlQuery = DcqlSamples.GetDcqlQueryWithClaimsets;
        var credentialQuery = dcqlQuery.CredentialQueries[0];
        var expectedClaimIds = credentialQuery.ClaimSets![0].Claims.Select(id => id.AsString()).ToArray();
        var credential = SdJwtSamples.GetIdCardCredential();

        // Act
        var sut = dcqlQuery.ProcessWith([credential]);

        // Assert
        sut.FlattenCandidates().Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();
                presentationCandidates.Should().HaveCount(1);
                foreach (var candidate in presentationCandidates)
                {
                    candidate.Identifier.Should().Be(credentialQuery.Id);
                }
                var claimsToDisclose = presentationCandidates[0].ClaimsToDisclose;
                claimsToDisclose.Match(
                    claims =>
                    {
                        var claimIds = claims.Select(c => c.Id!.AsString()).ToArray();
                        claimIds.Should().BeEquivalentTo(expectedClaimIds);
                    },
                    () => Assert.Fail("Expected claims to be returned, but got none.")
                );
            },
            () => Assert.Fail("Expected candidates to be returned, but got none.")
        );
    }

    [Fact]
    public void No_Claims_Are_Disclosed_When_Claims_in_Dcql_Query_Are_Absent()
    {
        // Arrange
        var dcqlQuery = DcqlSamples.GetDcqlQueryWithNoClaims;
        var credential = SdJwtSamples.GetIdCardCredential();

        // Act
        var sut = dcqlQuery.ProcessWith([credential]);

        // Assert
        sut.FlattenCandidates().Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();
                presentationCandidates.Should().HaveCount(1);
                foreach (var candidate in presentationCandidates)
                {
                    candidate.Identifier.Should().Be(dcqlQuery.CredentialQueries[0].Id);
                }
                var claimsToDisclose = presentationCandidates[0].ClaimsToDisclose;
                claimsToDisclose.IsNone.Should().BeTrue();
            },
            () => Assert.Fail("Expected candidates to be returned, but got none.")
        );
    }
} 
