using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Errors;

public record VciSessionIdError(string Value) : Error($"Invalid VciSessionId: {Value}");
