using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Json.Errors;

public record JTokenIsNotAnJObjectError(string Name, Exception E) 
    : Error($"The field '{Name}' is not an JObject.", E);
