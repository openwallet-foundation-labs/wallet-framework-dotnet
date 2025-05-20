using FluentAssertions;
using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.Samples;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql;

public class DcqlParsingTests
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
        cred.Id.AsString().Should().Be("idcard");
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
} 
