using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Functional;
using Xunit;

namespace WalletFramework.Core.Tests.Path;

public class ClaimPathTests
{
    private readonly object[] _claimPath = ["address", "street_address"];

    [Fact]
    public void Can_Create_ClaimPath()
    {
        // Act
        var claimPath = ClaimPath.FromObjects(_claimPath);

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
        // Arrange
        var claimPath = ClaimPath.FromObjects(path).UnwrapOrThrow();
        
        // Act
        var jsonPath = claimPath.ToJsonPath();

        // Assert
        Assert.Equal(expectedResult, jsonPath);
    }
}
