using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Errors;

public record VciSessionStateError(string Value) : Error($"Invalid VciSessionState: {Value}");
