using System.Diagnostics;
using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Jwk;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption.Implementations;

public class AuthorizationResponseEncryptionService
    (IHttpClientFactory httpClientFactory): IAuthorizationResponseEncryptionService
{
    public async Task<EncryptedAuthorizationResponse> Encrypt(
        AuthorizationResponse response,
        AuthorizationRequest request,
        Option<Nonce> mdocNonce)
    {
        var hasJwksUri = request.ClientMetadata?.JwksUri is not null;
        var hasJwksInMetadata = request.ClientMetadata?.JwkSet.IsSome ?? false;

        JsonWebKey verifierPubKey;
        switch (hasJwksUri, hasJwksInMetadata)
        {
            case (true, false):
            case (true, true):
                var httpClient = httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Clear();
                var httpResponseMessage = await httpClient.GetAsync(request.ClientMetadata!.JwksUri);
                var jwkSetJsonStr = await httpResponseMessage.Content.ReadAsStringAsync();
                verifierPubKey = JwkSet.FromJsonStr(jwkSetJsonStr).UnwrapOrThrow().GetFirst();
                break;
            case (false, true):
                verifierPubKey = request.ClientMetadata!.JwkSet.UnwrapOrThrow().GetFirst();
                break;
            default:
                throw new InvalidOperationException("Neither jwks or jwk_uri found");
        }
        
        return response.Encrypt(
            verifierPubKey,
            request.Nonce,
            request.ClientMetadata?.AuthorizationEncryptedResponseEnc,
            mdocNonce);
    }
}
