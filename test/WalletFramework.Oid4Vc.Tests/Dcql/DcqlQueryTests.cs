using FluentAssertions;
using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Tests.Dcql.Models;

namespace WalletFramework.Oid4Vc.Tests.Dcql;

public class DcqlQueryTests
{
    [Fact]
    public void Can_Parse_Dcql_Query()
    {
        var json = DcqlTestsDataProvider.GetJsonForTestCase();
        var dcqlQuery = JsonConvert.DeserializeObject<DcqlQuery>(json);

        dcqlQuery.Credentials.Length.Should().Be(5);

        dcqlQuery.Credentials[0].Id.Should().Be("pid");
        dcqlQuery.Credentials[0].Format.Should().Be("dc+sd-jwt");
        dcqlQuery.Credentials[0].Meta.Vcts[0].Should().Be("https://credentials.example.com/identity_credential");
        
        dcqlQuery.Credentials[0].Claims[0].Path.Length.Should().Be(1);
        dcqlQuery.Credentials[0].Claims[1].Path.Length.Should().Be(1);
        dcqlQuery.Credentials[0].Claims[2].Path.Length.Should().Be(2);
            
        dcqlQuery.CredentialSets.Length.Should().Be(2);
        dcqlQuery.CredentialSets[0].Purpose.Should().Be("Identification");
        dcqlQuery.CredentialSets[0].Options[0][0].Should().Be("pid");
        dcqlQuery.CredentialSets[0].Options[1][0].Should().Be("other_pid");
        dcqlQuery.CredentialSets[0].Options[2][0].Should().Be("pid_reduced_cred_1");
        dcqlQuery.CredentialSets[0].Options[2][1].Should().Be("pid_reduced_cred_2");
    }
}
