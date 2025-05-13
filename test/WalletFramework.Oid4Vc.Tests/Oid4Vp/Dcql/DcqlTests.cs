using FluentAssertions;
using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.Samples;
using WalletFramework.Oid4Vc.Tests.Samples;
using static WalletFramework.Oid4Vc.Oid4Vp.Dcql.DcqlFun;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql;

public class DcqlTests
{
    [Fact]
    public void Can_Parse_Dcql_Query()
    {
        var json = DcqlSamples.GetDcqlQueryAsJsonStr();
        var dcqlQuery = JsonConvert.DeserializeObject<DcqlQuery>(json)!;

        dcqlQuery.CredentialQueries.Length.Should().Be(5);

        dcqlQuery.CredentialQueries[0].Id.AsString().Should().Be("pid");
        dcqlQuery.CredentialQueries[0].Format.Should().Be("dc+sd-jwt");
        dcqlQuery.CredentialQueries[0].Meta!.Vcts!
            .First()
            .Should()
            .Be("https://credentials.example.com/identity_credential");

        dcqlQuery.CredentialQueries[0].Claims![0].Path.GetPathComponents().Length().Should().Be(1);
        dcqlQuery.CredentialQueries[0].Claims![1].Path.GetPathComponents().Length().Should().Be(1);
        dcqlQuery.CredentialQueries[0].Claims![2].Path.GetPathComponents().Length().Should().Be(2);

        dcqlQuery.CredentialSetQueries!.Length.Should().Be(2);
        dcqlQuery.CredentialSetQueries[0].Purpose.Should().Contain(x => x.Name == "Identification");
        dcqlQuery.CredentialSetQueries[0].Options[0].Ids[0].AsString().Should().Be("pid");
        dcqlQuery.CredentialSetQueries[0].Options[1].Ids[0].AsString().Should().Be("other_pid");
        dcqlQuery.CredentialSetQueries[0].Options[2].Ids[0].AsString().Should().Be("pid_reduced_cred_1");
        dcqlQuery.CredentialSetQueries[0].Options[2].Ids[1].AsString().Should().Be("pid_reduced_cred_2");
    }

    [Fact]
    public void Can_Parse_Query_With_Claim_Sets()
    {
        // Arrange
        const string query = DcqlSamples.QueryStrWithClaimSets;

        // Act
        var sut = JsonConvert.DeserializeObject<DcqlQuery>(query)!;

        // Assert
        sut.CredentialQueries.Length.Should().Be(1);
        var cred = sut.CredentialQueries[0];
        cred.Id.AsString().Should().Be("pid");
        cred.Format.Should().Be("dc+sd-jwt");
        cred.Meta!.Vcts!.Should().ContainSingle().Which.Should()
            .Be("ID-Card");
        cred.Claims!.Length.Should().Be(4);
        cred.Claims[0].Id!.AsString().Should().Be("a");
        cred.Claims[1].Id!.AsString().Should().Be("b");
        cred.Claims[2].Id!.AsString().Should().Be("c");
        cred.Claims[3].Id!.AsString().Should().Be("d");
        cred.ClaimSets!.Count.Should().Be(2);
        cred.ClaimSets![0].Claims.Select(c => c.AsString()).Should().BeEquivalentTo("a", "b", "d");
        cred.ClaimSets![1].Claims.Select(c => c.AsString()).Should().BeEquivalentTo("a", "c");
    }

    [Fact]
    public void Can_Process_Mdoc_Credential_Query()
    {
        var mdoc = MdocSamples.MdocRecord;
        var query = DcqlSamples.GetMdocGivenNameQuery();

        var sut = query.FindMatchingCandidates([mdoc]);

        sut.Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();
                presentationCandidates.Should().HaveCount(1);
                foreach (var candidate in presentationCandidates)
                {
                    candidate.Identifier.Should().Be(query.CredentialQueries[0].Id);
                    var expectedClaims = query.CredentialQueries[0].Claims!;
                    candidate.ClaimsToDisclose.Match(
                        claims =>
                        {
                            claims.Select(c => c.Path).Should().BeEquivalentTo(expectedClaims.Select(c => c.Path));
                        },
                        () => Assert.Fail("Expected all claims to be disclosed, but got none."));
                }

                var credentials = from candidate in presentationCandidates
                    from set in candidate.CredentialSetCandidates
                    from credential in set.Credentials
                    select credential;

                credentials.Should().Contain([mdoc]);
            },
            () => Assert.Fail()
        );
    }

    [Fact]
    public void Can_Process_Mdoc_Credential_Query_With_Multiple_Claims()
    {
        var mdoc = MdocSamples.MdocRecord;
        var query = DcqlSamples.GetMdocGivenNameAndFamilyNameQuery();

        var sut = query.FindMatchingCandidates([mdoc]);

        sut.Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();
                presentationCandidates.Should().HaveCount(1);
                foreach (var candidate in presentationCandidates)
                {
                    candidate.Identifier.Should().Be(query.CredentialQueries[0].Id);
                    var expectedClaims = query.CredentialQueries[0].Claims!;
                    candidate.ClaimsToDisclose.Match(
                        claims =>
                        {
                            claims.Select(c => c.Path).Should().BeEquivalentTo(expectedClaims.Select(c => c.Path));
                        },
                        () => Assert.Fail("Expected all claims to be disclosed, but got none."));
                }

                var credentials =
                    from candidate in presentationCandidates
                    from set in candidate.CredentialSetCandidates
                    from credential in set.Credentials
                    select credential;

                credentials.Should().Contain([mdoc]);
            },
            () => Assert.Fail()
        );
    }

    [Fact]
    public void Can_Process_Mdoc_Credential_Query_With_Multiple_Credentials_In_One_Candidate()
    {
        var mdoc1 = MdocSamples.MdocRecord;
        var mdoc2 = MdocSamples.MdocRecord;
        var query = DcqlSamples.GetMdocGivenNameQuery();

        var sut = query.FindMatchingCandidates([mdoc1, mdoc2]);

        sut.Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();
                presentationCandidates.Should().HaveCount(1);
                foreach (var candidate in presentationCandidates)
                {
                    candidate.Identifier.Should().Be(query.CredentialQueries[0].Id);
                    var expectedClaims = query.CredentialQueries[0].Claims!;
                    candidate.ClaimsToDisclose.Match(
                        claims =>
                        {
                            claims.Select(c => c.Path).Should().BeEquivalentTo(expectedClaims.Select(c => c.Path));
                        },
                        () => Assert.Fail("Expected all claims to be disclosed, but got none."));
                }

                var credentials = from candidate in presentationCandidates
                    from set in candidate.CredentialSetCandidates
                    from credential in set.Credentials
                    select credential;

                credentials.Should().Contain([mdoc1, mdoc2]);
            },
            () => Assert.Fail()
        );
    }

    [Fact]
    public void Can_Process_Query_That_Asks_For_SdJwt_And_Mdoc_At_The_Same_Time()
    {
        var mdoc = MdocSamples.MdocRecord;
        var sdJwt = SdJwtSamples.GetIdCardCredential();
        var query = DcqlSamples.GetMdocAndSdJwtFamilyNameQuery();

        var sut = query.FindMatchingCandidates([mdoc, sdJwt]);

        sut.Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();
                presentationCandidates.Should().HaveCount(2);
                foreach (var candidate in presentationCandidates)
                {
                    query.CredentialQueries.Select(q => q.Id.AsString()).Should().Contain(candidate.Identifier);
                    var expectedClaims = query.CredentialQueries.First(q => q.Id == candidate.Identifier).Claims!;
                    candidate.ClaimsToDisclose.Match(
                        claims =>
                        {
                            claims.Select(c => c.Path).Should().BeEquivalentTo(expectedClaims.Select(c => c.Path));
                        },
                        () => Assert.Fail($"Expected all claims to be disclosed for {candidate.Identifier}, but got none."));
                }

                var credentials = from candidate in presentationCandidates
                    from set in candidate.CredentialSetCandidates
                    from credential in set.Credentials
                    select credential;

                credentials.Should().Contain([mdoc, sdJwt]);
            },
            () => Assert.Fail()
        );
    }

    [Fact]
    public void Can_Process_SdJwt_Credential_Query()
    {
        var sdJwt = SdJwtSamples.GetIdCardCredential();
        var query = DcqlSamples.GetIdCardNationalitiesSecondIndexQuery();

        var sut = query.FindMatchingCandidates([sdJwt]);

        sut.Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();

                presentationCandidates.Should().HaveCount(1);
                foreach (var candidate in presentationCandidates)
                {
                    candidate.Identifier.Should().Be(query.CredentialQueries[0].Id);
                    var expectedClaims = query.CredentialQueries[0].Claims!;
                    candidate.ClaimsToDisclose.Match(
                        claims =>
                        {
                            claims.Select(c => c.Path).Should().BeEquivalentTo(expectedClaims.Select(c => c.Path));
                        },
                        () => Assert.Fail("Expected all claims to be disclosed, but got none."));
                }

                var credentials = from candidate in presentationCandidates
                    from set in candidate.CredentialSetCandidates
                    from credential in set.Credentials
                    select credential;

                credentials.Should().Contain([sdJwt]);
            },
            () => Assert.Fail()
        );
    }

    [Fact]
    public void Can_Process_SdJwt_Credential_Query_With_Multiple_Candidates()
    {
        var idCard = SdJwtSamples.GetIdCardCredential();
        var idCard2 = SdJwtSamples.GetIdCard2Credential();
        var query = DcqlSamples.GetIdCardAndIdCard2NationalitiesSecondIndexQuery();

        var sut = query.FindMatchingCandidates([idCard, idCard2]);

        sut.Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();
                presentationCandidates.Should().HaveCount(2);
                foreach (var candidate in presentationCandidates)
                {
                    query.CredentialQueries.Select(q => q.Id.AsString()).Should().Contain(candidate.Identifier);
                    var expectedClaims = query.CredentialQueries.First(q => q.Id == candidate.Identifier).Claims!;
                    candidate.ClaimsToDisclose.Match(
                        claims =>
                        {
                            claims.Select(c => c.Path).Should().BeEquivalentTo(expectedClaims.Select(c => c.Path));
                        },
                        () => Assert.Fail($"Expected all claims to be disclosed for {candidate.Identifier}, but got none."));
                }

                var credentials = from candidate in presentationCandidates
                    from set in candidate.CredentialSetCandidates
                    from credential in set.Credentials
                    select credential;

                credentials.Should().Contain([idCard, idCard2]);
            },
            () => Assert.Fail()
        );
    }

    [Fact]
    public void Can_Process_SdJwt_Credential_Query_With_Multiple_Credentials_In_One_Candidate()
    {
        var idCard = SdJwtSamples.GetIdCardCredential();
        var idCard2 = SdJwtSamples.GetIdCardCredential();
        var query = DcqlSamples.GetIdCardNationalitiesSecondIndexQuery();

        var sut = query.FindMatchingCandidates([idCard, idCard2]);

        sut.Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();
                presentationCandidates.Should().HaveCount(1);
                foreach (var candidate in presentationCandidates)
                {
                    candidate.Identifier.Should().Be(query.CredentialQueries[0].Id);
                    var expectedClaims = query.CredentialQueries[0].Claims!;
                    candidate.ClaimsToDisclose.Match(
                        claims =>
                        {
                            claims.Select(c => c.Path).Should().BeEquivalentTo(expectedClaims.Select(c => c.Path));
                        },
                        () => Assert.Fail("Expected all claims to be disclosed, but got none."));
                }

                var credentials = from candidate in presentationCandidates
                    from set in candidate.CredentialSetCandidates
                    from credential in set.Credentials
                    select credential;

                credentials.Should().Contain([idCard, idCard2]);
            },
            () => Assert.Fail()
        );
    }

    [Fact]
    public void No_Match_Returns_None()
    {
        var sdJwt = SdJwtSamples.GetIdCardCredential();
        var query = DcqlSamples.GetNoMatchErrorClaimPathQuery();

        var sut = query.FindMatchingCandidates([sdJwt]);

        sut.Match(
            _ => Assert.Fail(),
            () => { }
        );
    }

    [Fact]
    public void No_Match_Returns_None_For_Mdoc()
    {
        var mdoc = MdocSamples.MdocRecord;
        var query = DcqlSamples.GetNoMatchErrorClaimPathQuery();

        var sut = query.FindMatchingCandidates([mdoc]);

        sut.Match(
            _ => Assert.Fail(),
            () => { }
        );
    }

    [Fact]
    public void The_First_Matching_Claim_Set_Is_Disclosed()
    {
        // Arrange
        var dcqlQuery = DcqlSamples.GetDcqlQueryWithClaimsets;
        var credentialQuery = dcqlQuery.CredentialQueries[0];
        var expectedClaimIds = credentialQuery.ClaimSets![0].Claims.Select(id => id.AsString()).ToArray();
        var credential = SdJwtSamples.GetIdCardCredential();

        // Act
        var sut = dcqlQuery.FindMatchingCandidates([credential]);

        // Assert
        sut.Match(
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
        var sut = dcqlQuery.FindMatchingCandidates([credential]);

        // Assert
        sut.Match(
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
