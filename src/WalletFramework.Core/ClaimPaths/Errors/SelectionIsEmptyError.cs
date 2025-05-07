using WalletFramework.Core.ClaimPaths.Errors.Abstractions;

namespace WalletFramework.Core.ClaimPaths.Errors;

public record SelectionIsEmptyError() 
    : ClaimPathError("The selection is empty after processing the claims path pointer.");
