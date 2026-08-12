using Newtonsoft.Json;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vp.Dcql.CredentialQueries;

namespace WalletFramework.Oid4Vp.Tests.Dcql.CredentialSets;

public class CredentialQueryIdJsonConverterTests
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        Converters =
        {
            new CredentialQueryIdJsonConverter()
        }
    };

    [Fact]
    public void CanSerializeCredentialQueryId()
    {
        var id = CredentialQueryId.Create("id1").UnwrapOrThrow();
        var json = JsonConvert.SerializeObject(id, Settings);
        Assert.Equal("\"id1\"", json);
    }

    [Fact]
    public void CanDeserializeCredentialQueryId()
    {
        const string json = "\"id1\"";
        var id = JsonConvert.DeserializeObject<CredentialQueryId>(json, Settings);
        Assert.NotNull(id);
        Assert.Equal("id1", id.AsString());
    }

    [Fact]
    public void ThrowsOnInvalidCredentialQueryId()
    {
        const string invalidJson = "null";
        Assert.Throws<JsonSerializationException>(() =>
            JsonConvert.DeserializeObject<CredentialQueryId>(invalidJson, Settings));
    }
} 
