using LanguageExt;

namespace WalletFramework.Functional;

public readonly struct Validation<T>
{
    public Validation(Validation<Error, T> value)
    {
        Value = value;
    }
    
    public Validation<Error, T> Value { get; }
    
    public static implicit operator Validation<Error, T>(Validation<T> value) => value.Value;
    
    public static implicit operator Validation<T>(Validation<Error, T> value) => new(value);
    
    public static implicit operator Validation<T>(Error error) => new(Validation<Error, T>.Fail(Seq.create(error)));
    
    public static implicit operator Validation<T>(Seq<Error> errors) => new(Validation<Error, T>.Fail(errors));
    
    public static implicit operator Validation<T>(T value) => new(Validation<Error, T>.Success(value));
}

public delegate Validation<T> Validator<T>(T value);

public static class ValidationFun
{
    public static Validation<Func<T2, Func<T3, Func<T4, Func<T5, Func<T6, TR>>>>>> Apply<T1, T2, T3, T4, T5, T6, TR>(
        this Validation<Func<T1, T2, T3, T4, T5, T6, TR>> valF,
        Validation<T1> valT) =>
        Apply(valF.Select(Prelude.curry), valT);
    
    public static Validation<Func<T2, Func<T3, Func<T4, Func<T5, TR>>>>> Apply<T1, T2, T3, T4, T5, TR>(
        this Validation<Func<T1, T2, T3, T4, T5, TR>> valF,
        Validation<T1> valT) =>
        Apply(valF.Select(Prelude.curry), valT);
    
    public static Validation<Func<T2, Func<T3, Func<T4, TR>>>> Apply<T1, T2, T3, T4, TR>(
        this Validation<Func<T1, T2, T3, T4, TR>> valF,
        Validation<T1> valT) =>
        Apply(valF.Select(Prelude.curry), valT);
    
    public static Validation<Func<T2, Func<T3, TR>>> Apply<T1, T2, T3, TR>(
        this Validation<Func<T1, T2, T3, TR>> valF,
        Validation<T1> valT) =>
        Apply(valF.Select(Prelude.curry), valT);
    
    public static Validation<Func<T2, TR>> Apply<T1, T2, TR>(
        this Validation<Func<T1, T2, TR>> valF,
        Validation<T1> valT) =>
        Apply(valF.Select(Prelude.curry), valT);
    
    public static Validation<TR> Apply<T, TR>(
        this Validation<Func<T, TR>> valF,
        Validation<T> valT) =>
        valF.Value.Match(
            f =>
                valT.Value.Match(
                    t => Valid(f(t)),
                    errors => errors
                ),
            errors =>
                valT.Value.Match(
                    _ => errors,
                    errorsT => errors + errorsT
                )
        );
    
    public static T2 Match<TValue, T2>(
        this Validation<TValue> validation,
        Func<TValue, T2> valid,
        Func<Seq<Error>, T2> invalid) =>
        validation.Value.Match(valid, invalid);
    
    public static async Task<T2> Match<TValue, T2>(
        this Task<Validation<TValue>> validation,
        Func<TValue, Task<T2>> valid,
        Func<Seq<Error>, Task<T2>> invalid) =>
        await (await validation).Value.MatchAsync(valid, invalid);
    
    public static Unit Match<TValue>(
        this Validation<TValue> validation,
        Action<TValue> valid,
        Action<Seq<Error>> invalid) =>
        validation.Value.Match(valid, invalid);
    
    public static async Task<Validation<TReturn>> Select<TValue, TReturn>(
        this Task<Validation<TValue>> validation,
        Func<TValue, Task<TReturn>> task) =>
        await (await validation).Value.MatchAsync(
            async value => Valid(await task(value)),
            error => error
        );
    
    public static Validation<TResult> Select<TValue, TResult>(
        this Validation<TValue> validation,
        Func<TValue, TResult> func) =>
        validation.Value.Select(func);
    
    public static async Task<Validation<T3>> SelectMany<T1, T2, T3>(
        this Validation<T1> validation,
        Func<T1, Task<Validation<T2>>> bind,
        Func<T1, T2, T3> project)
    {
        var bindResult =
            await validation.Value.MatchAsync(
                async t => (await bind(t)).Value,
                error => error
            );
        
        return validation.Value.SelectMany(_ => bindResult, project);
    }
    
    public static async Task<Validation<T3>> SelectMany<T1, T2, T3>(
        this Task<Validation<T1>> validation,
        Func<T1, Task<Validation<T2>>> bind,
        Func<T1, T2, T3> project)
    {
        var validationValue = await validation;
        
        var bindResult =
            await validationValue.Value.MatchAsync(
                async t => (await bind(t)).Value,
                error => error
            );
        
        return validationValue.Value.SelectMany(_ => bindResult, project);
    }
    
    public static Validation<T3> SelectMany<T1, T2, T3>(
        this Validation<T1> validation,
        Func<T1, Validation<T2>> bind,
        Func<T1, T2, T3> project) =>
        new(validation.Value.SelectMany(t => bind(t).Value, project));
    
    public static Option<T> ToOption<T>(this Validation<T> validation) =>
        validation.Value.ToOption();

    public static Validation<IEnumerable<TR>> Traverse<T, TR>(
        this IEnumerable<Validation<T>> enumerable,
        Func<T,TR> func) =>
        enumerable
            .Select(validation => validation.Value)
            .Traverse(func);

    public static Validation<Dictionary<TKey, TValue>> Traverse<TKey, TValue>(
        this Dictionary<Validation<TKey>, Validation<TValue>> dict) where TKey : notnull
    {
        var createDict = new Func<IEnumerable<TKey>, IEnumerable<TValue>, Dictionary<TKey,TValue>>(
            (keys, values) =>
            {
                var result = new Dictionary<TKey, TValue>();
                var keysArray = keys.ToArray();
                var valuesArray = values.ToArray();
                for (var i = 0; i < keysArray.Length; i++)
                {
                    result.Add(keysArray[i], valuesArray[i]);
                }

                return result;
            }
        );

        var keys =
            dict.Keys.Select(validation => validation).Traverse(key => key);
        
        var values =
            dict.Values.Select(validation => validation).Traverse(value => value);

        return 
            Valid(createDict)
                .Apply(keys)
                .Apply(values);
    }

    public static Validation<T2> OnSuccess<T1, T2>(this Validation<T1> validation, Func<T1, Validation<T2>> onSucc) =>
        from t in validation
        from t2 in onSucc(t)
        select t2;

    public static Validation<T2> OnSuccess<T1, T2>(this Validation<T1> validation, Func<T1, T2> onSucc) =>
        from t in validation
        let t2 = onSucc(t)
        select t2;

    public static Validation<T> Invalid<T>(Seq<Error> errors) => Validation<Error, T>.Fail(errors);

    public static Validation<T> Valid<T>(T value) => value;
    
    public static Validation<T> Flatten<T>(this Validation<Validation<T>> stackedValidation) =>
        from outer in stackedValidation
        from inner in outer
        select inner;

    public static Validator<T> HarvestErrors<T>(IEnumerable<Validator<T>> validators)
        => t =>
        {
            var errors = validators
                .Select(validate => validate(t))
                .SelectMany(validation => validation.Match(
                    _ => Option<Seq<Error>>.None,
                    errors => errors
                    ))
                .SelectMany(seq => seq)
                .ToSeq();

            return errors.Count() == 0
                ? t
                : Invalid<T>(errors);
        };

    public static Validator<T> AggregateValidators<T>(this IEnumerable<Validator<T>> validators)
        => HarvestErrors(validators);
}
