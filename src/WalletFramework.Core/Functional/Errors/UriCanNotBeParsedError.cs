namespace WalletFramework.Core.Functional.Errors;

public record UriCanNotBeParsedError<T>() : Error($"The uri could not be parsed: `{nameof(T)}`");
