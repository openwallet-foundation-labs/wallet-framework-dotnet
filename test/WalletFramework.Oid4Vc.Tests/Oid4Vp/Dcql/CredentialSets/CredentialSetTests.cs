using FluentAssertions;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql;
using WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.Samples;
using WalletFramework.Oid4Vc.Tests.Samples;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Query;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.CredentialSets;

public class CredentialSetTests
{
    [Fact]
    public void Candidate_Query_Result_Can_Be_Built_Correctly_From_Dcql_Query_With_One_Credential_Set()
    {
        // Arrange
        var query = DcqlSamples.GetDcqlQueryWithOneCredentialSet;
        var idCard1 = SdJwtSamples.GetIdCardCredential();
        var idCard2 = SdJwtSamples.GetIdCard2Credential();
        var credentials = new List<ICredential> { idCard1, idCard2 };

        // Act
        var result = query.ProcessWith(credentials);

        // Assert
        result.Candidates.IsSome.Should().BeTrue();
        var sets = result.Candidates.IfNone([]);
        var setsList = sets.ToList();
        setsList.Count.Should().Be(1, "only the required set with idcard and idcard2 should be satisfied");
        var candidateSet = setsList[0];
        var candidatesList = candidateSet.Candidates.ToList();
        candidatesList.Count.Should().Be(2, "should have a candidate for each credential query in the set");
        candidateSet.IsRequired.Should().BeTrue();
        result.MissingCredentials.Match(
            list =>
            {
                list.Count.Should().Be(1);
                var first = list.First();
                first.GetIdentifier().Should().Be("idcard3");
            },
            () => Assert.Fail("Expected optional credential to be missing")
        );
    }

    [Fact]
    public void Candidate_Query_Result_Can_Be_Built_Correctly_From_Dcql_With_One_Credential_Set_And_Multiple_Options()
    {
        // Arrange
        var query = DcqlSamples.GetDcqlQueryWithOneCredentialSetAndMultipleOptions;
        var idCard = SdJwtSamples.GetIdCardCredential();
        var idCard2 = SdJwtSamples.GetIdCard2Credential();
        var idCard3 = SdJwtSamples.GetIdCard3Credential();
        var credentials = new List<ICredential> { idCard, idCard2, idCard3 };

        // Act
        var result = query.ProcessWith(credentials);

        // Assert
        result.Candidates.IsSome.Should().BeTrue();
        var sets = result.Candidates.IfNone([]);
        var setsList = sets.ToList();
        setsList.Count.Should().Be(1, "only one set should be satisfied by the provided credentials");
        var candidateSet = setsList[0];
        var candidatesList = candidateSet.Candidates.ToList();
        candidatesList.Count.Should().Be(2, "should have a candidate for each credential query in the set");
        candidateSet.IsRequired.Should().BeTrue();
    }
}
