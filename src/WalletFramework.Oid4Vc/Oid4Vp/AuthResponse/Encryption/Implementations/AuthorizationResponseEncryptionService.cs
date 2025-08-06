using LanguageExt;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption.Implementations;

public class AuthorizationResponseEncryptionService(IVerifierKeyService verifierKeyService)
    : IAuthorizationResponseEncryptionService
{
    public async Task<EncryptedAuthorizationResponse> Encrypt(
        AuthorizationResponse response,
        AuthorizationRequest request,
        Option<Nonce> mdocNonce)
    {
        var verifierPubKey = await verifierKeyService.GetPublicKey(request);
        
        var supportedAlgorithms = (request.ClientMetadata?.EncryptedResponseEncValuesSupported).AsOption().Match(
                encValues => encValues,
                () => (request.ClientMetadata?.AuthorizationEncryptedResponseEnc).AsOption().OnSome(encValue => new[] { encValue })
            );
        
        return response.Encrypt(
            verifierPubKey,
            request.Nonce,
            supportedAlgorithms,
            mdocNonce);
    }
}
