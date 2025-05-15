using WalletFramework.Core.ClaimPaths.Errors.Abstractions;

namespace WalletFramework.Core.ClaimPaths.Errors;

public record NamespaceNotFoundError(string Namespace)
    : ClaimPathError($"The namespace '{Namespace}' was not found in the document."); 