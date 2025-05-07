using FluentAssertions;
using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.Samples;

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

        dcqlQuery.CredentialQueries[0].Claims![0].Path!.Length.Should().Be(1);
        dcqlQuery.CredentialQueries[0].Claims![1].Path!.Length.Should().Be(1);
        dcqlQuery.CredentialQueries[0].Claims![2].Path!.Length.Should().Be(2);

        dcqlQuery.CredentialSetQueries!.Length.Should().Be(2);
        dcqlQuery.CredentialSetQueries[0].Purpose.Should().Contain(x => x.Name == "Identification");
        dcqlQuery.CredentialSetQueries[0].Options![0][0].Should().Be("pid");
        dcqlQuery.CredentialSetQueries[0].Options![1][0].Should().Be("other_pid");
        dcqlQuery.CredentialSetQueries[0].Options![2][0].Should().Be("pid_reduced_cred_1");
        dcqlQuery.CredentialSetQueries[0].Options![2][1].Should().Be("pid_reduced_cred_2");
    }
}
