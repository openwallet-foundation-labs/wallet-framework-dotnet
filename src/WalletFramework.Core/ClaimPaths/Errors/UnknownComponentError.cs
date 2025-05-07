using WalletFramework.Core.ClaimPaths.Errors.Abstractions;

namespace WalletFramework.Core.ClaimPaths.Errors;

public record UnknownComponentError()
    : ClaimPathError("The claim path component is unknown or unsupported.");
