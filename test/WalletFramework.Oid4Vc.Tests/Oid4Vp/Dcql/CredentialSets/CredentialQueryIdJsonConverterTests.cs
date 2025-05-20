using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialSets;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.CredentialSets;

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
