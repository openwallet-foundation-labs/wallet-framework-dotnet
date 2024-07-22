using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Cryptography.Abstractions;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Exceptions;
using WalletFramework.Oid4Vc.Oid4Vci.Extensions;

namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Implementations;

public class DPopHttpClient : IDPopHttpClient
{
    private const string ErrorCodeKey = "error";
    private const string InvalidGrantError = "invalid_grant";
    private const string UseDPopNonceError = "use_dpop_nonce";
    
    public DPopHttpClient(
        IHttpClientFactory httpClientFactory,
        IKeyStore keyStore,
        ILogger<DPopHttpClient> logger)
    {
        _keyStore = keyStore;
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
    }

    private readonly IKeyStore _keyStore;
    private readonly ILogger<DPopHttpClient> _logger;
    private readonly HttpClient _httpClient; 

    public async Task<DPopHttpResponse> Post(
        Uri requestUri,
        DPopConfig config,
        Func<HttpContent> getContent)
    {
        var dPop = await _keyStore.GenerateDPopProofOfPossessionAsync(
            config.KeyId,
            config.Audience,
            config.Nonce.ToNullable(),
            config.OAuthToken.ToNullable()?.AccessToken);

        var httpClient = config.OAuthToken.Match(
            token => _httpClient.WithDPopHeader(dPop).WithAuthorizationHeader(token),
            () => _httpClient.WithDPopHeader(dPop));
        
        var response = await httpClient.PostAsync(requestUri, getContent());
        
        await ThrowIfInvalidGrantError(response);
            
        var nonceStr = await GetDPopNonce(response);
        if (!string.IsNullOrEmpty(nonceStr))
        {
            config = config with { Nonce = new DPopNonce(nonceStr) };
            
            var newDpop = await _keyStore.GenerateDPopProofOfPossessionAsync(
                config.KeyId, 
                config.Audience, 
                config.Nonce.ToNullable(), 
                config.OAuthToken.ToNullable()?.AccessToken);
            
            httpClient.WithDPopHeader(newDpop);
            
            response = await httpClient.PostAsync(requestUri, getContent());
        }
            
        await ThrowIfInvalidGrantError(response);

        var message = await response.Content.ReadAsStringAsync();
            
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Http Request with DPop failed. Status Code is {response.StatusCode} with message: {message}");

        return new DPopHttpResponse(response, config);
    }

    private async Task ThrowIfInvalidGrantError(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var errorReason = string.IsNullOrEmpty(content)
            ? null
            : JObject.Parse(content)[ErrorCodeKey]?.ToString();

        if (response.StatusCode is System.Net.HttpStatusCode.BadRequest && errorReason == InvalidGrantError)
        {
            _logger.LogError("Error while sending request: {Content}", content);
            throw new Oid4VciInvalidGrantException(response.StatusCode);
        }
    }
    
    private static async Task<string?> GetDPopNonce(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var errorReason = string.IsNullOrEmpty(content) 
            ? null 
            : JObject.Parse(content)[ErrorCodeKey]?.ToString();
            
        if (response.StatusCode 
                is System.Net.HttpStatusCode.BadRequest 
                or System.Net.HttpStatusCode.Unauthorized
            && errorReason == UseDPopNonceError
            && response.Headers.TryGetValues("DPoP-Nonce", out var dPopNonce))
        {
            return dPopNonce?.FirstOrDefault();
        }

        return null;
    }
}
