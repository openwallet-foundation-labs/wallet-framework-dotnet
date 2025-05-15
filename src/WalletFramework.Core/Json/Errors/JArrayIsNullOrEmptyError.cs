using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Json.Errors;

public record JArrayIsNullOrEmptyError<T>() 
    : Error($"The JArray is null or empty for Type: `{nameof(T)}`"); 