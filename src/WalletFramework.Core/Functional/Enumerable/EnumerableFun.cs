namespace WalletFramework.Core.Functional.Enumerable;

public static class EnumerableFun
{
    public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => !enumerable.Any();
}
