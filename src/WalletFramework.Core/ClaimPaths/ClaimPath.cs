using WalletFramework.Core.ClaimPaths.Errors;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Path;
using LanguageExt;

namespace WalletFramework.Core.ClaimPaths;

public readonly struct ClaimPath
{
    private readonly IReadOnlyList<ClaimPathComponent> _components;

    private ClaimPath(IReadOnlyList<ClaimPathComponent> components)
    {
        _components = components;
    }

    public IReadOnlyList<ClaimPathComponent> GetPathComponents() => _components;

    public static Validation<ClaimPath> FromComponents(IEnumerable<ClaimPathComponent> components)
    {
        var list = components.ToList();
        if (list.Count == 0)
            return new ClaimPathIsEmptyError();
        return new ClaimPath(list);
    }

    public static Validation<ClaimPath> FromObjects(IEnumerable<object?> objects) =>
        from components in objects.TraverseAll(ClaimPathComponent.Create)
        from path in FromComponents(components)
        select path;
}

public static class ClaimPathFun
{
    public static JsonPath ToJsonPath(this ClaimPath claimPath)
    {
        var jsonPath = "$." + string.Join('.', claimPath.GetPathComponents().Select(x =>
        {
            if (x.IsKey) return x.AsKey();
            if (x.IsIndex) return x.AsIndex()?.ToString();
            return null;
        }).Where(x => x is not null));
        return JsonPath.ValidJsonPath(jsonPath).UnwrapOrThrow();
    }
}
