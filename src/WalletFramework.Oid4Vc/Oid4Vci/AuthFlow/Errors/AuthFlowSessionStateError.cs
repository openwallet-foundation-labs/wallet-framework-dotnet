using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Errors;

public record AuthFlowSessionStateError(string Value) : Error($"Invalid AuthFlowSessionState: {Value}");
