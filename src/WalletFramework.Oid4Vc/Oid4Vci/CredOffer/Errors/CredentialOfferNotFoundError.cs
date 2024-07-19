using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Errors;

public record CredentialOfferNotFoundError()
    : Error("Neither an embedded credential offer or a credential offer uri could be found");
