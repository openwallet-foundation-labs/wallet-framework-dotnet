using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.ClaimPaths.Errors;

namespace WalletFramework.Core.ClaimPaths;

public record ClaimPathSelection
{
    private ClaimPathSelection(IEnumerable<JToken> values) => Values = values;

    private IEnumerable<JToken> Values { get; }

    public IEnumerable<JToken> GetValues() => Values;

    public static Validation<ClaimPathSelection> Create(IEnumerable<JToken> values)
    {
        var arr = values as JToken[] ?? [.. values];
        return arr.Any() ? new ClaimPathSelection(arr) : new SelectionIsEmptyError();
    }
}

public static class ClaimPathSelectionFun
{
    public static Validation<ClaimPathSelection> SelectObjectKey(ClaimPathSelection selection, string key)
    {
        if (selection.GetValues().Any(token => token is not JObject))
            return new SelectedElementIsNotAnObjectError();

        var newSelection = selection
            .GetValues()
            .Cast<JObject>()
            .SelectMany(obj => obj.TryGetValue(key, out var value)
                ? [value]
                : Array.Empty<JToken>());

        return ClaimPathSelection.Create(newSelection);
    }

    public static Validation<ClaimPathSelection> SelectArrayIndex(ClaimPathSelection selection, int index)
    {
        if (selection.GetValues().Any(token => token is not JArray))
            return new SelectedElementIsNotAnArrayError();

        var arrays = selection.GetValues().Cast<JArray>().ToArray();
        if (arrays.Any(array => index < 0 || index >= array.Count))
            return new SelectedElementDoesNotExistInArrayError();

        var newSelection = arrays.Select(array => array[index]);
        return ClaimPathSelection.Create(newSelection);
    }

    public static Validation<ClaimPathSelection> SelectAllArrayElements(ClaimPathSelection selection)
    {
        if (selection.GetValues().Any(token => token is not JArray))
            return new SelectedElementIsNotAnArrayError();

        var newSelection = selection
            .GetValues()
            .Cast<JArray>()
            .SelectMany(array => array.Children());

        return ClaimPathSelection.Create(newSelection);
    }
}
