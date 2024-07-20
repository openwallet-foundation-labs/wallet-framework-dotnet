using OneOf;
using WalletFramework.Core.Cryptography.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using static Newtonsoft.Json.JsonConvert;

namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.Implementations;

internal class TokenService : ITokenService
{
    private readonly IDPopHttpClient _dPopHttpClient;
    private readonly IKeyStore _keyStore;
    private readonly HttpClient _httpClient;

    public TokenService(
        IDPopHttpClient dPopHttpClient,
        IHttpClientFactory httpClientFactory,
        IKeyStore keyStore)
    {
        _dPopHttpClient = dPopHttpClient;
        _keyStore = keyStore;
        _httpClient = httpClientFactory.CreateClient();
    }
    
    public async Task<OneOf<OAuthToken, DPopToken>> RequestToken(
        TokenRequest tokenRequest,
        AuthorizationServerMetadata metadata)
    {
        if (metadata.IsDPoPSupported)
        {
            var keyId = await _keyStore.GenerateKey();
            
            var config = new DPopConfig(keyId, metadata.TokenEndpoint);

            var uri = new Uri(metadata.TokenEndpoint);
            
            var result = await _dPopHttpClient.Post(
                uri,
                config,
                tokenRequest.ToFormUrlEncoded);
            
            var token = DeserializeObject<OAuthToken>(await result.ResponseMessage.Content.ReadAsStringAsync()) 
                        ?? throw new InvalidOperationException("Failed to deserialize the token response");

            var dPop = new DPop.Models.DPop(result.Config);
            
            return new DPopToken(token, dPop);
        }
        else
        {
            var response = await _httpClient.PostAsync(
                metadata.TokenEndpoint,
                tokenRequest.ToFormUrlEncoded());
            
            var token = DeserializeObject<OAuthToken>(await response.Content.ReadAsStringAsync()) 
                        ?? throw new InvalidOperationException("Failed to deserialize the token response");
        
            return token;
        }
    }
}
