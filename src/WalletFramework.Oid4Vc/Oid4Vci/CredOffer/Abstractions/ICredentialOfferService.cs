using WalletFramework.Core.Functional;
using WalletFramework.Core.Localization;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Abstractions;

public interface ICredentialOfferService
{
    public Task<Validation<CredentialOffer>> ProcessCredentialOffer(Uri credentialOffer, Locale language);
}
