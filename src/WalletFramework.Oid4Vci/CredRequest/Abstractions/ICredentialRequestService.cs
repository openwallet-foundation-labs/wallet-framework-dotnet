using LanguageExt;
using OneOf;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vci.Authorization.DPop.Models;
using WalletFramework.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vci.CredConfiguration.Models;
using WalletFramework.Oid4Vci.CredOffer.Models;
using WalletFramework.Oid4Vci.CredResponse;
using WalletFramework.Oid4Vci.Issuer.Models;

namespace WalletFramework.Oid4Vci.CredRequest.Abstractions;

public interface ICredentialRequestService
{
    public Task<Validation<IEnumerable<CredentialResponse>>> RequestCredentials(
        KeyValuePair<CredentialConfigurationId, SupportedCredentialConfiguration> configurationPair,
        IssuerMetadata issuerMetadata,
        OneOf<OAuthToken, DPopToken> token,
        Option<int> specVersion);
}
