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
}
