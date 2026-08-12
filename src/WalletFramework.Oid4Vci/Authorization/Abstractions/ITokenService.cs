using LanguageExt;
using OneOf;
using WalletFramework.Oid4Vci.Authorization.DPop.Models;
using WalletFramework.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vci.CredentialNonce.Models;
using WalletFramework.WalletAttestations;

namespace WalletFramework.Oid4Vci.Authorization.Abstractions;

public interface ITokenService
{
    public Task<OneOf<OAuthToken, DPopToken>> RequestToken(
        AuthorizationServerMetadata metadata,
        Option<ClientAttestation> clientAttestation,
        Option<CredentialNonceEndpoint> credentialNonceEndpoint,
        TokenRequest tokenRequest);
}
