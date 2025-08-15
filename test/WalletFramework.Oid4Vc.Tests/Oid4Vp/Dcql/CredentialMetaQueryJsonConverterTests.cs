using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql;

public class CredentialMetaQueryJsonConverterTests
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        Converters =
        {
            new CredentialMetaQueryJsonConverter()
        }
    };
    
    [Fact]
    public void CanSerializeCredentialMetaQuery()
    {
        // Arrange
        var credentialMetaQuery = new CredentialMetaQuery()
        {
            Doctype = "DocumentType1"
        };
        
        // Act
        var json = JsonConvert.SerializeObject(credentialMetaQuery, Settings);
        
        // Assert
        Assert.NotNull(json);
    }

    [Fact]
    public void CanDeserializeCredentialMetaQuery()
    {
        //Arrange
        var jsonString = new JObject()
        {
            ["vct_values"] = new JArray(){"VerifiableCredentialType1", "VerifiableCredentialType2"}
        }.ToString();
        
        //Act
        var credentialMetaQuery = JsonConvert.DeserializeObject<CredentialMetaQuery>(jsonString, Settings);
        
        //Assert
        Assert.Equal(["VerifiableCredentialType1", "VerifiableCredentialType2"], credentialMetaQuery!.Vcts);
    }
    
    [Fact]
    public void ThrowsOnInvalidCredentialMetaQuery_VctValuesAndDoctypeValueIsNotMutuallyExclusive()
    {
        // Arrange
        var invalidJson = new JObject()
        {
            ["vct_values"] = new JArray(){"VerifiableCredentialType1", "VerifiableCredentialType2"},
            ["doctype_value"] = "DocumentType1"
        };
        
        // Act & Assert
        Assert.Throws<JsonSerializationException>(() =>
            JsonConvert.DeserializeObject<CredentialMetaQuery>(invalidJson.ToString(), Settings));
    }

    [Fact]
    public void ThrowsOnInvalidCredentialMetaQuery()
    {
        // Arrange
        var invalidJson = new JObject()
        {
            ["some"] = new JArray(){1, 2},
            ["another"] = 1
        };
        
        // Act & Assert
        Assert.Throws<JsonSerializationException>(() =>
            JsonConvert.DeserializeObject<CredentialMetaQuery>(invalidJson.ToString(), Settings));
    }
}
