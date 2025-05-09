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

        dcqlQuery.CredentialQueries[0].Id.Should().Be("pid");
        dcqlQuery.CredentialQueries[0].Format.Should().Be("dc+sd-jwt");
        dcqlQuery.CredentialQueries[0].Meta!.Vcts!
            .First()
            .Should()
            .Be("https://credentials.example.com/identity_credential");

        dcqlQuery.CredentialQueries[0].Claims![0].Path!.GetPathComponents().Length().Should().Be(1);
        dcqlQuery.CredentialQueries[0].Claims![1].Path!.GetPathComponents().Length().Should().Be(1);
        dcqlQuery.CredentialQueries[0].Claims![2].Path!.GetPathComponents().Length().Should().Be(2);

        dcqlQuery.CredentialSetQueries!.Length.Should().Be(2);
        dcqlQuery.CredentialSetQueries[0].Purpose.Should().Contain(x => x.Name == "Identification");
        dcqlQuery.CredentialSetQueries[0].Options![0][0].Should().Be("pid");
        dcqlQuery.CredentialSetQueries[0].Options![1][0].Should().Be("other_pid");
        dcqlQuery.CredentialSetQueries[0].Options![2][0].Should().Be("pid_reduced_cred_1");
        dcqlQuery.CredentialSetQueries[0].Options![2][1].Should().Be("pid_reduced_cred_2");
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
    public void Can_Parse_Query_With_Claim_Sets()
    {
        // Arrange
        const string query = DcqlSamples.QueryStrWithClaimSets;

        // Act
        var sut = JsonConvert.DeserializeObject<DcqlQuery>(query)!;

        // Assert
        sut.CredentialQueries.Length.Should().Be(1);
        var cred = sut.CredentialQueries[0];
        cred.Id.Should().Be("pid");
        cred.Format.Should().Be("dc+sd-jwt");
        cred.Meta!.Vcts!.Should().ContainSingle().Which.Should().Be("https://credentials.example.com/identity_credential");
        cred.Claims!.Length.Should().Be(5);
        cred.Claims[0].Id!.AsString().Should().Be("a");
        cred.Claims[1].Id!.AsString().Should().Be("b");
        cred.Claims[2].Id!.AsString().Should().Be("c");
        cred.Claims[3].Id!.AsString().Should().Be("d");
        cred.Claims[4].Id!.AsString().Should().Be("e");
        cred.ClaimSets!.Length().Should().Be(2);
        cred.ClaimSets![0].Claims.Select(c => c.AsString()).Should().BeEquivalentTo(["a", "c", "d", "e"]);
        cred.ClaimSets![1].Claims.Select(c => c.AsString()).Should().BeEquivalentTo(["a", "b", "e"]);
    }

    [Fact]
    public void The_First_Matching_Claim_Set_Is_Prioritized()
    {
        // Arrange
        var dcqlQuery = DcqlSamples.GetDcqlQueryWithClaimsets;
        var credentialQuery = dcqlQuery.CredentialQueries[0];
        var expectedClaimIds = credentialQuery.ClaimSets![0].Claims.Select(id => id.AsString()).ToArray();

        // Act
        var sut = credentialQuery.GetClaimsToDisclose();

        // Assert
        sut.Match(
            claims =>
            {
                var claimIds = claims.Select(c => c.Id!.AsString()).ToArray();
                claimIds.Should().BeEquivalentTo(expectedClaimIds);
            },
            () => Assert.Fail("Expected claims to be returned, but got none.")
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

                var credentials = from candidate in presentationCandidates
                                  from set in candidate.CredentialSetCandidates
                                  from credential in set.Credentials
                                  select credential;

                credentials.Should().Contain([mdoc, sdJwt]);
            },
            () => Assert.Fail()
        );
    }
}
