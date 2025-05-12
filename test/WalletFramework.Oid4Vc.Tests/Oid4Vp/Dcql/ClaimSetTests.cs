using FluentAssertions;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.Samples;
using WalletFramework.Core.Functional;
using WalletFramework.Core.ClaimPaths;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql;

public class ClaimSetTests
{
    [Fact]
    public void Can_Parse_Claim_Set()
    {
        // Arrange
        var json = DcqlSamples.ClaimSetSampleJson;
        var jArray = JArray.Parse(json);

        // Act
        var validation = ClaimSetFun.Validate(jArray);

        // Assert
        validation.Match(
            claimSet =>
            {
                claimSet.Claims.Select(c => c.AsString()).Should().BeEquivalentTo("a", "b", "c");
            },
            errors => Assert.Fail("Validation should succeed")
        );
    }

    [Fact]
    public void Can_Parse_Multiple_Claim_Sets()
    {
        // Arrange
        var json = DcqlSamples.MultipleClaimSetsSampleJson;
        var jArray = JArray.Parse(json);

        // Act
        var validation = ClaimSetFun.ValidateMany(jArray);

        // Assert
        validation.Match(
            claimSets =>
            {
                var claimSetsList = claimSets.ToList();
                claimSetsList.Should().HaveCount(2);
                claimSetsList[0].Claims.Select(c => c.AsString()).Should().BeEquivalentTo("a", "b");
                claimSetsList[1].Claims.Select(c => c.AsString()).Should().BeEquivalentTo("c", "d");
            },
            errors => Assert.Fail("Validation should succeed")
        );
    }

    [Fact]
    public void Can_Match_Claims_To_ClaimSet()
    {
        // Arrange
        var dummyComponent = ClaimPathComponent.Create(JToken.FromObject("dummy")).UnwrapOrThrow();
        var dummyPath = ClaimPath.FromComponents([dummyComponent]).UnwrapOrThrow();
        var claimQueries = new[]
        {
            new ClaimQuery { Id = ClaimIdentifier.Validate("a").UnwrapOrThrow(), Path = dummyPath },
            new ClaimQuery { Id = ClaimIdentifier.Validate("b").UnwrapOrThrow(), Path = dummyPath },
            new ClaimQuery { Id = ClaimIdentifier.Validate("c").UnwrapOrThrow(), Path = dummyPath },
            new ClaimQuery { Id = ClaimIdentifier.Validate("d").UnwrapOrThrow(), Path = dummyPath }
        };
        var claimSets = new[]
        {
            new ClaimSet([ClaimIdentifier.Validate("a").UnwrapOrThrow(), ClaimIdentifier.Validate("b").UnwrapOrThrow()]),
            new ClaimSet([ClaimIdentifier.Validate("c").UnwrapOrThrow(), ClaimIdentifier.Validate("d").UnwrapOrThrow()]),
        };

        // Act
        var sut = claimQueries.ProcessSets(claimSets).ToList();

        // Assert
        sut.Should().HaveCount(2);
        sut[0].Select(q => q.Id!.AsString()).Should().BeEquivalentTo("a", "b");
        sut[1].Select(q => q.Id!.AsString()).Should().BeEquivalentTo("c", "d");
    }
} 
