using WalletFramework.Core.Functional;

namespace WalletFramework.Core.ClaimPaths.Errors.Abstractions;

public abstract record ClaimPathError : Error
{
    protected ClaimPathError(string message) : base(message)
    {
    }
}
