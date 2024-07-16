using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.Errors;

public record AuthorizationServerIdError(string Value, Exception E) : Error($"The AuthorizationServerId could not be parsed. Value is `{Value}`", E);
