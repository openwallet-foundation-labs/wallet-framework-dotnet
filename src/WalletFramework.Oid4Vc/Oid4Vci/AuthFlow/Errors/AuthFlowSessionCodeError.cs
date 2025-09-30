using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Errors;

public record AuthFlowSessionCodeError(string Value) : Error($"Invalid Issuance Session code parameter: {Value}");
