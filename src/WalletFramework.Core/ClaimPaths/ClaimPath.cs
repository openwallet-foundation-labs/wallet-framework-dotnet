using System.Text;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.ClaimPaths.Errors;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Path;

namespace WalletFramework.Core.ClaimPaths;

[JsonConverter(typeof(ClaimPathJsonConverter))]
public readonly struct ClaimPath
{
    private readonly IReadOnlyList<ClaimPathComponent> _components;

    private ClaimPath(IReadOnlyList<ClaimPathComponent> components) => _components = components;

    public IReadOnlyList<ClaimPathComponent> GetPathComponents() => _components;

    public static Validation<ClaimPath> FromComponents(IEnumerable<ClaimPathComponent> components)
    {
        var list = components.ToList();
        if (list.Count == 0)
            return new ClaimPathIsEmptyError();
        return new ClaimPath(list);
    }

    public static Validation<ClaimPath> FromJArray(JArray array) =>
        from components in array.TraverseAll(ClaimPathComponent.Create)
        from path in FromComponents(components)
        select path;

    public static JArray ToJArray(ClaimPath claimPath)
    {
        var array = new JArray();
        foreach (var component in claimPath.GetPathComponents())
        {
            component.Match(
                key =>
                {
                    array.Add(new JValue(key));
                    return Unit.Default;
                },
                index =>
                {
                    array.Add(new JValue(index));
                    return Unit.Default;
                },
                _ =>
                {
                    array.Add(JValue.CreateNull());
                    return Unit.Default;
                }
            );
        }

        return array;
    }
}

public static class ClaimPathFun
{
    public static JsonPath ToJsonPath(this ClaimPath claimPath)
    {
        var jsonPath = new StringBuilder();
        jsonPath.Append('$');

        foreach (var component in claimPath.GetPathComponents())
        {
            component.Match(
                key =>
                {
                    jsonPath.Append($".{key}");
                    return Unit.Default;
                },
                integer =>
                {
                    jsonPath.Append($"[{integer}]");
                    return Unit.Default;
                },
                _ =>
                {
                    jsonPath.Append("[*]");
                    return Unit.Default;
                });
        }

        return JsonPath.ValidJsonPath(jsonPath.ToString()).UnwrapOrThrow();
    }

    public static Validation<ClaimPathSelection> ProcessWith(this ClaimPath path, JObject jObject) =>
        path.GetPathComponents().Aggregate(
            ClaimPathSelection.Create([(JToken)jObject]),
            (validation, component) => validation.OnSuccess(selection =>
                component.Match(
                    s => ClaimPathSelectionFun.SelectObjectKey(selection, s),
                    i => ClaimPathSelectionFun.SelectArrayIndex(selection, i),
                    _ => ClaimPathSelectionFun.SelectAllArrayElements(selection))));
}
