using WalletFramework.Core.ClaimPaths.Errors;
using WalletFramework.Core.Functional;
using OneOf;

namespace WalletFramework.Core.ClaimPaths;

public sealed record ClaimPathComponent
{
    private readonly OneOf<string, int, SelectAllElementsInArrayComponent> _value;

    private ClaimPathComponent(OneOf<string, int, SelectAllElementsInArrayComponent> value) => _value = value;

    public static Validation<ClaimPathComponent> Create(object? value) =>
        value switch
        {
            string s => string.IsNullOrEmpty(s)
                ? new UnknownComponentError() 
                : new ClaimPathComponent(s),
            int i => new ClaimPathComponent(i),
            null => new ClaimPathComponent(new SelectAllElementsInArrayComponent()),
            _ => new UnknownComponentError().ToInvalid<ClaimPathComponent>()
        };

    public T Match<T>(
        Func<string, T> onKey,
        Func<int, T> onIndex,
        Func<SelectAllElementsInArrayComponent, T> onSelectAll) =>
        _value.Match(onKey, onIndex, onSelectAll);

    public bool IsKey => _value.IsT0;

    public bool IsIndex => _value.IsT1;

    public string? AsKey() => _value.TryPickT0(out var key, out _) ? key : null;

    public int? AsIndex() => _value.TryPickT1(out var idx, out _) ? idx : null;
} 