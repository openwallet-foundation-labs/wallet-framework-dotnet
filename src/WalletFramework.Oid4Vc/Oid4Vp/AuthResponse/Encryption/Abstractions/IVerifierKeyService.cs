using Microsoft.IdentityModel.Tokens;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption.Abstractions;

public interface IVerifierKeyService
{
    Task<JsonWebKey> GetPublicKey(AuthorizationRequest request);
} 