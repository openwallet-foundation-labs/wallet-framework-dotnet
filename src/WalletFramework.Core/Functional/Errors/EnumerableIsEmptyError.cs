namespace WalletFramework.Core.Functional.Errors;

public record EnumerableIsEmptyError<T>() : Error($"The enumerable with items of type `{typeof(T).Name}` is empty");
