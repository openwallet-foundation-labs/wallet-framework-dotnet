using WalletFramework.Core.ClaimPaths.Errors.Abstractions;

namespace WalletFramework.Core.ClaimPaths.Errors;

public record SelectedElementIsNotAnObjectError()
    : ClaimPathError("The selected element is not an object.");
