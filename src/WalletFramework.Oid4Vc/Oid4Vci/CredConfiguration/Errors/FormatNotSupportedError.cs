using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Errors;

public record FormatNotSupportedError(string Value) : Error($"The given format `{Value}` is not supported");
