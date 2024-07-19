using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Json.Errors;

public record InvalidJsonError(string Json, Exception E)
    : Error($"The JSON could not be parsed. JSON Value is `{Json}`", E);
