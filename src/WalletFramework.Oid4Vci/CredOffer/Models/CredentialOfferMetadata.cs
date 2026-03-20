using WalletFramework.Oid4Vci.Issuer.Models;

namespace WalletFramework.Oid4Vci.CredOffer.Models;

public record CredentialOfferMetadata(CredentialOffer CredentialOffer, IssuerMetadata IssuerMetadata);
