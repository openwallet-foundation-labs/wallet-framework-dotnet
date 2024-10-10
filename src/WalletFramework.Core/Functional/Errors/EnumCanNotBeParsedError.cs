namespace WalletFramework.Core.Functional.Errors;

public record EnumCanNotBeParsedError<T>(string value) : Error($"The enum value `{value}` could not be parsed: `{nameof(T)}`");
