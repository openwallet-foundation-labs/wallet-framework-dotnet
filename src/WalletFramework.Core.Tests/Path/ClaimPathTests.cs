using WalletFramework.Core.Functional;
using WalletFramework.Core.Path;
using Xunit;

namespace WalletFramework.Core.Tests.Path;

public class ClaimPathTests
{
    private readonly string?[] _claimPath;

    public ClaimPathTests()
    {
        _claimPath = ["address", "street_address"];
    }
    
    [Fact]
    public void Can_Create_ClaimPath()
    {
        // Act
        var claimPath = ClaimPath.ValidClaimPath(_claimPath);

        // Assert
        Assert.True(claimPath.IsSuccess);
    }
    
    [Fact]
    public void Can_Convert_ClaimPath_To_JsonPath()
    {
        // Arrange
        var claimPath = ClaimPath.ValidClaimPath(_claimPath).UnwrapOrThrow();
        
        // Act
        var jsonPath = claimPath.ToJsonPath();

        // Assert
        Assert.Equal("$.address.street_address", jsonPath);
    }
}
