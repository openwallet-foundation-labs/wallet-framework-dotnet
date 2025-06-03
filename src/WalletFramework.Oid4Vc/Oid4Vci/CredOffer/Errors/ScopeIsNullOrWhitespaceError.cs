using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Errors;

public record ScopeIsNullOrWhitespaceError()
    : Error("The scope is null or whitespace");
