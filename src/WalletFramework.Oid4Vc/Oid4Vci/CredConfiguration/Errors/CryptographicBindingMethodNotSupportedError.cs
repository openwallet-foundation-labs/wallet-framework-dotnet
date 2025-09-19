using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Errors;

public record CryptographicBindingMethodNotSupportedError(string Value) : Error($"The CryptographicBindingMethod: `{Value}` is not supported");
