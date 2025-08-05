using LanguageExt;
using WalletFramework.Core.Functional.Enumerable;

namespace WalletFramework.Core.Functional;

public static class OptionFun
{
    public static Option<T> Some<T>(T value) => value;
    
    public static Option<T> None<T>() => Option<T>.None;
    
    public static Option<T> ParseOption<T>(T? value) => value ?? Option<T>.None;

    public static Option<T2> OnSome<T1, T2>(this Option<T1> option, Func<T1, Option<T2>> t2Func) =>
        from t1 in option
        from t2 in t2Func(t1)
        select t2;
    
    public static Option<T2> OnSome<T1, T2>(this Option<T1> option, Func<T1, T2> t2Func) =>
        from t1 in option
        let t2 = t2Func(t1)
        select t2;

    public static T? ToNullable<T>(this Option<T> option) =>
        option.MatchUnsafe(
            Some: value => value,
            None: () => default);

    public static T Fallback<T>(this Option<T> option, Func<T> fallbackFunc)
    {
        var f = fallbackFunc();
        return option.Fallback(f);
    }

    public static T Fallback<T>(this Option<T> option, T fallback) =>
        option.Match(
            t => t,
            () => fallback
        );

    /// <summary>
    ///     Traverses an enumerable for option
    /// </summary>
    /// <returns>
    /// <para>A option of an enumerable of every item.</para>
    /// <para>In case the enumerable is empty, the traverse will return None</para>
    /// </returns>
    /// <remarks>The traverse only succeeds when every item of the enumerable returns Some. If you want to
    /// ignore all the None items and only keep the Some items use <see cref="TraverseAny{T,TR}"/></remarks>
    public static Option<IEnumerable<TR>> TraverseAll<T, TR>(
        this IEnumerable<T> enumerable,
        Func<T, Option<TR>> optionFunc)
    {
        var list = enumerable.ToList();
        if (list.IsEmpty())
            return Option<IEnumerable<TR>>.None;
        
        return list
            .Select(optionFunc)
            .Traverse(t => t);
    }
    
    /// <summary>
    ///    Traverses an enumerable for option
    /// </summary>
    /// <returns>
    /// <para>A option of an enumerable which contains the items where the optionFunc returned Some</para>
    /// </returns>
    /// <remarks>
    /// The traverse will ignore the None items and only keep Some items. If you want the traverse to
    /// only return Some when everything is Some use <see cref="TraverseAll{T,TR}"/></remarks>
    public static Option<IEnumerable<TR>> TraverseAny<T, TR>(
        this IEnumerable<T> enumerable,
        Func<T, Option<TR>> optionFunc)
    {
        var items = enumerable.ToList();
        if (items.IsEmpty())
            return Option<IEnumerable<TR>>.None;
        
        var traverse = items
            .Select(optionFunc)
            .Where(validation => validation.IsSome)
            .Traverse(validation => validation);

        return traverse.OnSome(traversedItems =>
        {
            var list = traversedItems.ToList();
            return list.Any() ? list : Option<IEnumerable<TR>>.None;
        });
    }

    public static Option<IEnumerable<T>> AsOption<T>(this IEnumerable<T> enumerable) =>
        // ReSharper disable once PossibleMultipleEnumeration
        enumerable.IsEmpty()
            ? Option<IEnumerable<T>>.None
            // ReSharper disable once PossibleMultipleEnumeration
            : Some(enumerable);

    public static Option<T> AsOption<T>(this T? value) where T : class =>
        value != null ? Option<T>.Some(value) : Option<T>.None;
    
    public static T UnwrapOrThrow<T>(this Option<T> option, Exception e) =>
        option.Match(
            t => t,
            () => throw e);
    
    public static T UnwrapOrThrow<T>(this Option<T> option) =>
        option.UnwrapOrThrow(new InvalidOperationException("Option is None"));
}
