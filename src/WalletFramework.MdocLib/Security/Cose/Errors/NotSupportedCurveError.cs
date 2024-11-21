using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Security.Cose.Errors;

public record NotSupportedCurveError(string CurveValue) : Error(
    $"The curve value {CurveValue} is not supported. Currently only P-256 (1) value is supported");
