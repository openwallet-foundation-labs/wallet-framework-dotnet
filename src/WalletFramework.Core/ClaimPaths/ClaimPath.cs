using WalletFramework.Core.Functional;
using WalletFramework.Core.Path;

namespace WalletFramework.Core.ClaimPaths;

public readonly struct ClaimPath
{
    private ClaimPath(string?[] path) => Value = path;

    public string?[] Value { get; }

    public static implicit operator string?[](ClaimPath claimPath) => claimPath.Value;

    public static Validation<ClaimPath> ValidClaimPath(string?[] path) => new ClaimPath(path);
}

public static class ClaimPathFun
{
    public static JsonPath ToJsonPath(this ClaimPath claimPath)
    {
        var jsonPath = $"$.{string.Join('.', ((string?[])claimPath).Where(x => x is not null))}";
        return JsonPath.ValidJsonPath(jsonPath).UnwrapOrThrow();
    }
}
