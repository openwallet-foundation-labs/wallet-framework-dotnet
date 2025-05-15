using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Functional;
using Xunit;

namespace WalletFramework.Core.Tests.Path;

public class ClaimPathTests
{
    private readonly JArray _claimPath = ["address", "street_address"];
    
    [Fact]
    public void Can_Create_ClaimPath()
    {
        // Act
        var claimPath = ClaimPath.FromJArray(_claimPath);
    
        // Assert
        Assert.True(claimPath.IsSuccess);
    }
    
    [Theory]
    [InlineData(new[] {"name"}, "$.name")]
    [InlineData(new[] {"address"}, "$.address")]
    [InlineData(new[] {"address", "street_address"}, "$.address.street_address")]
    [InlineData(new[] {"degree", null}, "$.degree")]
    public void Can_Convert_ClaimPath_To_JsonPath(object[] path, string expectedResult)
    {
        var jArray = new JArray(path);
        
        // Arrange
        var claimPath = ClaimPath.FromJArray(jArray).UnwrapOrThrow();
        
        // Act
        var jsonPath = claimPath.ToJsonPath();
    
        // Assert
        Assert.Equal(expectedResult, jsonPath);
    }

    [Fact]
    public void ClaimPathJsonConverter_Can_ReadJson()
    {
        // Arrange
        var json = "[\"address\",\"street_address\"]";
        var settings = new JsonSerializerSettings { Converters = { new ClaimPathJsonConverter() } };
        
        // Act
        var claimPath = JsonConvert.DeserializeObject<ClaimPath>(json, settings);
        
        // Assert
        var expected = ClaimPath.FromJArray(new JArray("address", "street_address")).UnwrapOrThrow();
        Assert.Equal(expected.GetPathComponents(), claimPath.GetPathComponents());
    }

    [Fact]
    public void ClaimPathJsonConverter_Can_WriteJson()
    {
        // Arrange
        var claimPath = ClaimPath.FromJArray(new JArray("address", "street_address")).UnwrapOrThrow();
        var settings = new JsonSerializerSettings { Converters = { new ClaimPathJsonConverter() } };
        
        // Act
        var json = JsonConvert.SerializeObject(claimPath, settings);
        
        // Assert
        Assert.Equal("[\"address\",\"street_address\"]", json);
    }
}
