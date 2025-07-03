using LanguageExt;
using OneOf;
using WalletFramework.Oid4Vc.ClientAttestation;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.Abstractions;

public interface ITokenService
{
    public Task<OneOf<OAuthToken, DPopToken>> RequestToken(
        TokenRequest tokenRequest,
        AuthorizationServerMetadata metadata,
        Option<ClientAttestationDetails> clientAttestationDetails,
        Option<CredentialNonceEndpoint> credentialNonceEndpoint);
}
