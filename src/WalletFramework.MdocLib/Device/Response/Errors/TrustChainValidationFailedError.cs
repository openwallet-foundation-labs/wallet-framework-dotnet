using LanguageExt;
using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Device.Response.Errors;

public record TrustChainValidationFailedError : Error
{
    public TrustChainValidationFailedError() : base("The TrustChain validation failed")
    {
    }

    public TrustChainValidationFailedError(Exception exception) : base("The TrustChain validation failed", exception)
    {
    }
}
