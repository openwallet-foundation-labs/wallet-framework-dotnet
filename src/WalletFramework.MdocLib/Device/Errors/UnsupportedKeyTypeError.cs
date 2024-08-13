using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Device.Errors;

public record UnsupportedKeyTypeError : Error
{
    public UnsupportedKeyTypeError(int kty) : base($"The key type with a value of {kty} is not suppported")
    {
    }
}
