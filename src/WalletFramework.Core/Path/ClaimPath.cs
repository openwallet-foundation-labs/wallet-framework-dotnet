using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Path;

public readonly struct ClaimPath
{
    private string?[] Value { get; }

    private ClaimPath(string?[] path) => Value = path;
    
    public static implicit operator string?[](ClaimPath claimPath) => claimPath.Value;

    public static Validation<ClaimPath> ValidClaimPath(string?[] path)
    {
        return new ClaimPath(path);
    }
}

public static class ClaimPathFun
{
    public static JsonPath ToJsonPath(this ClaimPath claimPath)
    {
        var jsonPath = $"$.{string.Join('.', ((string?[])claimPath).Where(x => x is not null))}";
        return JsonPath.ValidJsonPath(jsonPath).UnwrapOrThrow();
    }
}
