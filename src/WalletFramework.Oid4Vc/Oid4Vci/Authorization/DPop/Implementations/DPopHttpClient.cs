using System.Security.Cryptography;
using System.Text;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Cryptography.Abstractions;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.ClientAttestation;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Exceptions;
using WalletFramework.Oid4Vc.Oid4Vci.Extensions;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Implementations;

public class DPopHttpClient : IDPopHttpClient
{
    private const string ErrorCodeKey = "error";
    private const string InvalidGrantError = "invalid_grant";
    private const string UseDPopNonceError = "use_dpop_nonce";
    
    public DPopHttpClient(
        IHttpClientFactory httpClientFactory,
        IKeyStore keyStore,
        ISdJwtSigner sdJwtSigner,
        ILogger<DPopHttpClient> logger)
    {
        _keyStore = keyStore;
        _sdJwtSigner = sdJwtSigner;
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
    }

    private readonly IKeyStore _keyStore;
    private readonly ISdJwtSigner _sdJwtSigner;
    private readonly ILogger<DPopHttpClient> _logger;
    private readonly HttpClient _httpClient; 

    public async Task<DPopHttpResponse> Post(
        Uri requestUri,
        DPopConfig config,
        Option<CombinedWalletAttestation> combinedWalletAttestation,
        Func<HttpContent> getContent)
    {
        var dPop = await GenerateDPopHeaderAsync(
            config.KeyId,
            config.Audience,
            config.Nonce.ToNullable(),
            config.OAuthToken.ToNullable()?.AccessToken);
        
        var httpClient = config.OAuthToken.Match(
            token => _httpClient.WithDPopHeader(dPop).WithAuthorizationHeader(token),
            () => _httpClient.WithDPopHeader(dPop));

        combinedWalletAttestation.IfSome(attestation => httpClient.AddClientAttestationPopHeader(attestation));
        
        var response = await httpClient.PostAsync(requestUri, getContent());
        
        await ThrowIfInvalidGrantError(response);
            
        var freshNonce = await GetAuthorizationServerProvidedDPopNonce(response).Match(
            authServerProvidedNonce => authServerProvidedNonce,
            () => GetResourceServerProvidedDPopNonce(response));

        await freshNonce.Match(
            async nonce =>
            {
                config = config with { Nonce = new DPopNonce(nonce) };

                var newDpop = await GenerateDPopHeaderAsync(
                    config.KeyId,
                    config.Audience,
                    config.Nonce.ToNullable(),
                    config.OAuthToken.ToNullable()?.AccessToken);

                httpClient.WithDPopHeader(newDpop);

                response = await httpClient.PostAsync(requestUri, getContent());

                config = response.Headers.TryGetValues("DPoP-Nonce", out var refreshedDpopNonce)
                    ? config with { Nonce = new DPopNonce(refreshedDpopNonce?.First()!) }
                    : config;
            },
            () => Task.CompletedTask
        );
            
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
    
    private static async Task<Option<string>> GetAuthorizationServerProvidedDPopNonce(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        var errorReason = string.IsNullOrEmpty(responseContent)
            ? Option<string>.None
            : JObject.Parse(responseContent)[ErrorCodeKey]?.ToString();
        
        return errorReason.Match(
            reason =>
            {
                if (response.StatusCode 
                        is System.Net.HttpStatusCode.BadRequest 
                        or System.Net.HttpStatusCode.Unauthorized
                    && reason == UseDPopNonceError
                    && response.Headers.TryGetValues("DPoP-Nonce", out var dPopNonce))
                {
                    return dPopNonce.FirstOrDefault();
                }

                return Option<string>.None;
            },
            () => Option<string>.None);
    }
    
    private static Option<string> GetResourceServerProvidedDPopNonce(HttpResponseMessage response)
    {
        if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized
            && response.Headers.TryGetValues("WWW-Authenticate", out var wwwAuthenticateValues)
            && response.Headers.TryGetValues("DPoP-Nonce", out var dPopNonceValues))
        {
            return wwwAuthenticateValues
                .Find(headerValue => headerValue.Contains("DPoP")
                                     && headerValue.Contains("error=\"use_dpop_nonce\""))
                .Match(_ => dPopNonceValues.FirstOrDefault(),
                    () => Option<string>.None);
        }
        
        return Option<string>.None;
    }
    
    private async Task<string> GenerateDPopHeaderAsync(KeyId keyId, string audience, string? nonce, string? accessToken)
    {
        var header = new Dictionary<string, object>
        {
            { "alg", "ES256" },
            { "typ", "dpop+jwt" }
        };
            
        var publicKey = await _keyStore.GetPublicKey(keyId);
        header["jwk"] = publicKey.ToObj();

        string? ath = null;
        if (!string.IsNullOrEmpty(accessToken))
        {
            var sha256 = SHA256.Create();
            ath = Base64UrlEncoder.Encode(sha256.ComputeHash(Encoding.UTF8.GetBytes(accessToken)));
        }
            
        var dPopPayload = new
        {
            jti = Guid.NewGuid().ToString(),
            htm = "POST",
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            htu = new Uri(audience).GetLeftPart(UriPartial.Path),
            nonce,
            ath
        };
            
        return await _sdJwtSigner.CreateSignedJwt(header, dPopPayload, keyId);
    }
}
