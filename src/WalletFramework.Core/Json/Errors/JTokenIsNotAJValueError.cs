using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Json.Errors;

// ReSharper disable once InconsistentNaming
public record JTokenIsNotAJValueError(string Token, Exception E)
    : Error($"The token `{Token}` could not be transformed into a JValue", E);
