using WalletFramework.Core.Functional;

namespace WalletFramework.Credentials;

public record FormatNotSupportedError(string Value) : Error($"The given format `{Value}` is not supported");
