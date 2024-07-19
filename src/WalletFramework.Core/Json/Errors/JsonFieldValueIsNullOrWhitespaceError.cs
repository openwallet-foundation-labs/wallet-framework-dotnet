using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Json.Errors;

public record JsonFieldValueIsNullOrWhitespaceError(string Name) 
    : Error($"The value of the field `{Name}` is null or empty");
