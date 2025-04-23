using LanguageExt;
using OneOf;
using WalletFramework.Core.Cryptography.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Models;
using static Newtonsoft.Json.JsonConvert;

namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.Implementations;

internal class TokenService(
    IDPopHttpClient dPopHttpClient,
    IHttpClientFactory httpClientFactory,
    ICNonceService cNonceService,
    IKeyStore keyStore)
    : ITokenService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task<OneOf<OAuthToken, DPopToken>> RequestToken(
        TokenRequest tokenRequest,
        AuthorizationServerMetadata metadata,
        Option<CNonceEndpoint> nonceEndpoint)
    {
        if (metadata.IsDPoPSupported)
        {
            var keyId = await keyStore.GenerateKey(isPermanent: false);
            
            var config = new DPopConfig(keyId, metadata.TokenEndpoint);

            var uri = new Uri(metadata.TokenEndpoint);
            
            var result = await dPopHttpClient.Post(
                uri,
                config,
                tokenRequest.ToFormUrlEncoded);
            
            var token = DeserializeObject<OAuthToken>(await result.ResponseMessage.Content.ReadAsStringAsync()) 
                        ?? throw new InvalidOperationException("Failed to deserialize the token response");

            await nonceEndpoint.IfSomeAsync(async endpoint =>
            {
                var nonce = await cNonceService.GetCredentialNonce(endpoint);
                token = token with { CNonce = nonce.Value };
            });
            
            return new DPopToken(
                token, 
                new DPop.Models.DPop(result.Config)
                );
        }
        else
        {
            var response = await _httpClient.PostAsync(
                metadata.TokenEndpoint,
                tokenRequest.ToFormUrlEncoded());
            
            var token = DeserializeObject<OAuthToken>(await response.Content.ReadAsStringAsync()) 
                        ?? throw new InvalidOperationException("Failed to deserialize the token response");
            
            await nonceEndpoint.IfSomeAsync(async endpoint =>
            {
                var nonce = await cNonceService.GetCredentialNonce(endpoint);
                token = token with { CNonce = nonce.Value };
            });
            
            return token;
        }
    }
}
