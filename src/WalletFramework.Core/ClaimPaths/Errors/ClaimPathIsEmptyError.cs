using WalletFramework.Core.Functional;

namespace WalletFramework.Core.ClaimPaths.Errors;

public record ClaimPathIsEmptyError() : Error("ClaimPath is not allowed to be empty");
