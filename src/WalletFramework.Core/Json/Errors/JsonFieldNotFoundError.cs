using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Json.Errors;

public record JsonFieldNotFoundError(string FieldName) : Error($"The field '{FieldName}' was not found.");
