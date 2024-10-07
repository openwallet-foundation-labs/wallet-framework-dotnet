namespace WalletFramework.Core.Functional.Errors;

public record UriCanNotBeParsedError(Exception E) : Error("The uri could not be parsed", E);
