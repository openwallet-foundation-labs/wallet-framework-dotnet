namespace WalletFramework.Core.Functional.Errors;

public record StringIsNullOrWhitespaceError<T>() : Error($"The string is null or whitespace for Type: `{nameof(T)}`");
