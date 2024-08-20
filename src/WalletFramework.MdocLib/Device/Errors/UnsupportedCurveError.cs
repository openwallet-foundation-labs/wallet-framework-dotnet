using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Device.Errors;

public record UnsupportedCurveError : Error
{
    public UnsupportedCurveError(int crv) : base($"The curve with a value of {crv} is not supported")
    {
    }
}
