using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialSets;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.CredentialSets;

public class CredentialSetJsonConverterTests
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        Converters = { new CredentialSetJsonConverter() }
    };

    [Fact]
    public void CanSerializeCredentialSetOptionList()
    {
        var list = new List<CredentialSetOption>
        {
            new([
                CredentialQueryId.Create("id1").UnwrapOrThrow(),
                CredentialQueryId.Create("id2").UnwrapOrThrow()
            ]),
            new([
                CredentialQueryId.Create("id3").UnwrapOrThrow()
            ])
        };
        var json = JsonConvert.SerializeObject(list, Settings);
        Assert.Equal("[[\"id1\",\"id2\"],[\"id3\"]]", json);
    }

    [Fact]
    public void CanDeserializeCredentialSetOptionList()
    {
        const string json = "[[\"id1\",\"id2\"],[\"id3\"]]";
        var deserialized = JsonConvert.DeserializeObject<IReadOnlyList<CredentialSetOption>>(json, Settings);
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.Count);
        Assert.Equal(["id1", "id2"], deserialized[0].Ids.Select(id => id.AsString()));
        Assert.Equal(["id3"], deserialized[1].Ids.Select(id => id.AsString()));
    }

    [Fact]
    public void ThrowsOnInvalidCredentialSetOptionList()
    {
        const string invalidJson = "[[123, null], [\"id4\"]]";
        Assert.Throws<JsonSerializationException>(() =>
            JsonConvert.DeserializeObject<IReadOnlyList<CredentialSetOption>>(invalidJson, Settings));
    }
} 
