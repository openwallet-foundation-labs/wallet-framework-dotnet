using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Json.Errors;

public record JTokenIsNotAnJArrayError(string Name, Exception E) 
    : Error($"The field '{Name}' is not an JArray.", E);
