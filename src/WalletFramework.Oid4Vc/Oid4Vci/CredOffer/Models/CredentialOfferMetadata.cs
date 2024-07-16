using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;

public record CredentialOfferMetadata(CredentialOffer CredentialOffer, IssuerMetadata IssuerMetadata);
