using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.Samples;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql;

public class ClaimSetJsonConverterTests
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        Converters = { new ClaimSetJsonConverter() }
    };

    [Fact]
    public void CanSerializeClaimSetList()
    {
        var list = new List<ClaimSet>
        {
            new([
                ClaimIdentifier.Validate("a").UnwrapOrThrow(),
                ClaimIdentifier.Validate("b").UnwrapOrThrow()
            ]),
            new([
                ClaimIdentifier.Validate("c").UnwrapOrThrow(),
                ClaimIdentifier.Validate("d").UnwrapOrThrow()
            ])
        };
        var json = JsonConvert.SerializeObject(list, Settings);
        Assert.Equal("[[\"a\",\"b\"],[\"c\",\"d\"]]", json);
    }

    [Fact]
    public void CanDeserializeClaimSetList()
    {
        var json = DcqlSamples.MultipleClaimSetsSampleJson; // "[[\"a\", \"b\"], [\"c\", \"d\"]]"
        var deserialized = JsonConvert.DeserializeObject<IReadOnlyList<ClaimSet>>(json, Settings);
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.Count);
        Assert.Equal(["a", "b"], deserialized[0].Claims.Select(claim => claim.AsString()));
        Assert.Equal(["c", "d"], deserialized[1].Claims.Select(claim => claim.AsString()));
    }

    [Fact]
    public void CanSerializeSingleClaimSet()
    {
        var list = new List<ClaimSet>
        {
            new([
                ClaimIdentifier.Validate("a").UnwrapOrThrow(),
                ClaimIdentifier.Validate("b").UnwrapOrThrow(),
                ClaimIdentifier.Validate("c").UnwrapOrThrow()
            ])
        };
        var json = JsonConvert.SerializeObject(list, Settings);
        Assert.Equal("[[\"a\",\"b\",\"c\"]]", json);
    }

    [Fact]
    public void CanDeserializeSingleClaimSet()
    {
        var json = DcqlSamples.ClaimSetSampleJson; // "[\"a\", \"b\", \"c\"]"
        // Wrap it in an array to match the IReadOnlyList<ClaimSet> format
        var wrappedJson = $"[{json}]";
        var deserialized = JsonConvert.DeserializeObject<IReadOnlyList<ClaimSet>>(wrappedJson, Settings);
        Assert.NotNull(deserialized);
        Assert.Single(deserialized);
        Assert.Equal(["a", "b", "c"], deserialized[0].Claims.Select(claim => claim.AsString()));
    }

    [Fact]
    public void ThrowsOnInvalidClaimSetList()
    {
        const string invalidJson = "[[123, null], [\"valid\"]]";
        Assert.Throws<JsonSerializationException>(() =>
            JsonConvert.DeserializeObject<IReadOnlyList<ClaimSet>>(invalidJson, Settings));
    }

    [Fact]
    public void ThrowsOnEmptyClaimSet()
    {
        const string emptyClaimSetJson = "[[]]";
        Assert.Throws<JsonSerializationException>(() =>
            JsonConvert.DeserializeObject<IReadOnlyList<ClaimSet>>(emptyClaimSetJson, Settings));
    }
} 
