using Microsoft.IdentityModel.Tokens;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Jwk;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption.Implementations;

public class VerifierKeyService(IHttpClientFactory httpClientFactory) : IVerifierKeyService
{
    public async Task<JsonWebKey> GetPublicKey(AuthorizationRequest request)
    {
        var hasJwksUri = request.ClientMetadata?.JwksUri is not null;
        var hasJwksInMetadata = request.ClientMetadata?.JwkSet.IsSome ?? false;

        return (hasJwksUri, hasJwksInMetadata) switch
        {
            (true, false) or (true, true) => await GetKeyFromJwksUri(request.ClientMetadata!.JwksUri!),
            (false, true) => request.ClientMetadata!.JwkSet.UnwrapOrThrow().GetEcP256Jwk(),
            _ => throw new InvalidOperationException("Neither jwks or jwk_uri found")
        };
    }

    private async Task<JsonWebKey> GetKeyFromJwksUri(string jwksUri)
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Clear();
        var httpResponseMessage = await httpClient.GetAsync(jwksUri);
        var jwkSetJsonStr = await httpResponseMessage.Content.ReadAsStringAsync();
        return JwkSet.FromJsonStr(jwkSetJsonStr).UnwrapOrThrow().GetEcP256Jwk();
    }
} 
