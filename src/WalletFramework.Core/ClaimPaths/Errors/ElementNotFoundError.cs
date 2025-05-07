using WalletFramework.Core.ClaimPaths.Errors.Abstractions;

namespace WalletFramework.Core.ClaimPaths.Errors;

public record ElementNotFoundError(string Namespace, string ElementId)
    : ClaimPathError($"The element '{ElementId}' was not found in the namespace '{Namespace}'."); 