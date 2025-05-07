using WalletFramework.Core.ClaimPaths.Errors.Abstractions;

namespace WalletFramework.Core.ClaimPaths.Errors;

public record SelectedElementDoesNotExistInArrayError()
    : ClaimPathError("The selected element does not exist in the array.");
