using LanguageExt;
using WalletFramework.Oid4Vp.Models;

namespace WalletFramework.Oid4Vp.AuthResponse.Encryption.Abstractions;

public interface IAuthorizationResponseEncryptionService
{
    public Task<EncryptedAuthorizationResponse> Encrypt(
        AuthorizationResponse response,
        AuthorizationRequest request,
        Option<Nonce> mdocNonce);
}
