using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Device.Errors;

public record InvalidVersionError(string Expected, string Actual) : Error(
    $"Invalid Version. Expected is {Expected}, Actual is {Actual}");
