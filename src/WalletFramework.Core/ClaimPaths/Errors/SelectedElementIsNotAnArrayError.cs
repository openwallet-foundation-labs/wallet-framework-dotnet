using WalletFramework.Core.ClaimPaths.Errors.Abstractions;

namespace WalletFramework.Core.ClaimPaths.Errors;

public record SelectedElementIsNotAnArrayError()
    : ClaimPathError("The selected element is not an array.");
