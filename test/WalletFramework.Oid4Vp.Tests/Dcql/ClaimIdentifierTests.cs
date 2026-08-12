using FluentAssertions;
using Newtonsoft.Json;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vp.Dcql.Models;

namespace WalletFramework.Oid4Vp.Tests.Dcql;

public class ClaimIdentifierTests
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        Converters = { new ClaimIdentifierJsonConverter() }
    };

    [Theory]
    [InlineData("\"test-claim\"")]
    [InlineData("{\"value\":\"test-claim\"}")]
    public void Can_Read_ClaimIdentifier_From_Json(string json)
    {
        var claimIdentifier = JsonConvert.DeserializeObject<ClaimIdentifier>(json, Settings);
        claimIdentifier.Should().NotBeNull();
        claimIdentifier!.AsString().Should().Be("test-claim");
    }

    [Fact]
    public void Can_Write_ClaimIdentifier_To_Json()
    {
        var claimIdentifier = ClaimIdentifier.Validate("test-claim").UnwrapOrThrow();
        var json = JsonConvert.SerializeObject(claimIdentifier, Settings);
        json.Should().Be("{\"value\":\"test-claim\"}");
    }

    [Fact]
    public void Can_Read_Null_Values()
    {
        var claimIdentifier = JsonConvert.DeserializeObject<ClaimIdentifier>("null", Settings);
        claimIdentifier.Should().BeNull();
    }
} 
