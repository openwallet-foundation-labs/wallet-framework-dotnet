using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Errors;

public record CredentialOfferHasNoQueryParameterError(Exception E) 
    : Error("The credential offer contains no query parameters", E);
