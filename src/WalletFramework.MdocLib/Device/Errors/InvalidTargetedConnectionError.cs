using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Device.Errors;

public record InvalidTargetedConnectionError(int Expected, int Actual) : Error(
    $"Expected targeted connection value of {Expected}, but got {Actual}");
