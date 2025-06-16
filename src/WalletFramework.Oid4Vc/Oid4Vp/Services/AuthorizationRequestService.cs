using System.Net.Http.Headers;
using System.Web;
using Hyperledger.Aries.Utils;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.Errors;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.Abstractions;
using static WalletFramework.Oid4Vc.Oid4Vp.Models.RequestObject;
using static WalletFramework.Oid4Vc.Oid4Vp.Models.ClientIdScheme.ClientIdSchemeValue;
using static Newtonsoft.Json.JsonConvert;
using static WalletFramework.Oid4Vc.Oid4Vp.Models.AuthorizationRequest;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

public class AuthorizationRequestService(
    IHttpClientFactory httpClientFactory,
    IRpAuthService rpAuthService) : IAuthorizationRequestService
{
    private const string RequestUriMethodGet = "get";
    private const string RequestUriMethodPost = "post";
    
    public async Task<Validation<AuthorizationRequestCancellation, AuthorizationRequest>> GetAuthorizationRequest(
        AuthorizationRequestUri authorizationRequestUri) =>
        await authorizationRequestUri.Value.Match(
            async reference =>
            {
                var requestObjectValidation = await GetRequestObject(reference);
                return await requestObjectValidation.MatchAsync(
                    async requestObject =>
                    {
                        var authRequest = requestObject.ToAuthorizationRequest();
                        var rpAuthResult = await rpAuthService.Authenticate(requestObject);

                        Validation<AuthorizationRequestCancellation, AuthorizationRequest> result = authRequest with
                        {
                            RpAuthResult = rpAuthResult
                        };

                        return result;
                    },
                    seq => seq
                );
            },
            async value => await GetAuthRequestByValue(value));

    private async Task<Validation<AuthorizationRequestCancellation, RequestObject>> GetRequestObject(
        AuthorizationRequestByReference authRequestByReference)
    {
        var requestObjectValidation = await FetchRequestObject(authRequestByReference);

        return await requestObjectValidation.MatchAsync(
            async requestObject =>
            {
                var authRequest = requestObject.ToAuthorizationRequest();
                var clientMetadataOption = 
                    await FetchClientMetadata(authRequest).OnException(_ => Option<ClientMetadata>.None);

                var error = new InvalidRequestError($"Client ID Scheme {requestObject.ClientIdScheme} is not supported");
    
                Validation<AuthorizationRequestCancellation, RequestObject> result = 
                    requestObject.ClientIdScheme.Value switch
                    {
                        X509SanDns => requestObject
                            .ValidateJwtSignature()
                            .ValidateTrustChain()
                            .ValidateSanName()
                            .WithX509()
                            .WithClientMetadata(clientMetadataOption),
                        RedirectUri => requestObject
                            .WithClientMetadata(clientMetadataOption),
                        //TODO: Remove Did in the future (kept for now for compatibility)
                        Did => requestObject
                            .WithClientMetadata(clientMetadataOption),
                        _ => new AuthorizationRequestCancellation(authRequest.GetResponseUriMaybe(), [error])
                    };

                return result;
            },
            seq => seq);
    }

    private async Task<Validation<AuthorizationRequestCancellation, AuthorizationRequest>> GetAuthRequestByValue(
        AuthorizationRequestByValue authRequestByValue)
    {
        var queryParams = HttpUtility.ParseQueryString(authRequestByValue.RequestUri.Query);

        var jObject = new JObject();
        foreach (var key in queryParams.AllKeys)
        {
            var value = queryParams[key];

            if (string.IsNullOrEmpty(value)) 
                continue;
                    
            try
            {
                jObject[key] = JToken.Parse(value);
            }
            catch (JsonReaderException)
            {
                jObject[key] = value;
            }
        }

        if (jObject.TryGetValue("presentation_definition_uri", out var presentationDefinitionUri))
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Clear();
            
            var requestObjectJson = await httpClient.GetStringAsync(presentationDefinitionUri.ToString());
            jObject["presentation_definition"] = JToken.Parse(requestObjectJson);
        }
                
        var jsonString = SerializeObject(jObject);
        
        return await CreateAuthorizationRequest(jsonString).MatchAsync(async authRequest =>
            {
                var clientMetadataOption = 
                    await FetchClientMetadata(authRequest).OnException(_ => Option<ClientMetadata>.None);
                    
                var error = new InvalidRequestError($"Client ID Scheme {authRequest.ClientIdScheme} is not supported");
            
                Validation<AuthorizationRequestCancellation, AuthorizationRequest> result = 
                    authRequest.ClientIdScheme.Value switch
                    {
                        RedirectUri => authRequest.WithClientMetadata(clientMetadataOption),
                        _ => new AuthorizationRequestCancellation(authRequest.GetResponseUriMaybe(), [error])
                    };

                return result;
            },
            seq => seq);
    }

    private async Task<Validation<AuthorizationRequestCancellation, RequestObject>> FetchRequestObject(AuthorizationRequestByReference authRequestByReference)
    {
        return await authRequestByReference.RequestUriMethod.Match<Task<Validation<AuthorizationRequestCancellation, RequestObject>>>(
            async method =>
            {
                return method.ToLowerInvariant() switch
                {
                    RequestUriMethodGet => await FetchRequestObjectViaGet(authRequestByReference),
                    RequestUriMethodPost => await FetchRequestObjectViaPost(authRequestByReference),
                    _ => new AuthorizationRequestCancellation(Option<Uri>.None, [new InvalidRequestUriMethodError($"Unsupported request_uri_method: '{method}'.")])
                };
            },
            async () => await FetchRequestObjectViaGet(authRequestByReference));
    }
        
    private async Task<Validation<AuthorizationRequestCancellation, RequestObject>> FetchRequestObjectViaPost(AuthorizationRequestByReference authRequestByReference)
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/oauth-authz-req+jwt"));
        
        var walletNonce = Base64UrlEncoder.Encode(Guid.NewGuid().ToString());
        var keyValuePairs = new List<KeyValuePair<string, string>>();
        keyValuePairs.Add(new KeyValuePair<string, string>("wallet_nonce", walletNonce));
        keyValuePairs.Add(new KeyValuePair<string, string>("wallet_metadata", new JObject()
        { 
            ["vp_formats_supported"] = new JObject() 
            {
                ["dc+sd-jwt"] = new JObject() 
                { 
                    ["sd-jwt_alg_values"] = new JArray(){ "ES256", "ES384", "ES512", "RS256" }, 
                    ["kb-jwt_alg_values"] = new JArray(){ "ES256" } 
                },
                ["mso_mdoc"] = new JObject() 
                { 
                    ["issuerauth_alg_values"] = new JArray(){ "ES256" }, 
                    ["deviceauth_alg_values"] = new JArray(){ "ES256" } 
                }
            } 
        }.ToString()));
        
        var response = await httpClient.PostAsync(authRequestByReference.RequestUri, new FormUrlEncodedContent(keyValuePairs));
        response.EnsureSuccessStatusCode();
        var stringContent = await response.Content.ReadAsStringAsync();

        return FromStr(stringContent, walletNonce);
    }
    
    private async Task<Validation<AuthorizationRequestCancellation, RequestObject>> FetchRequestObjectViaGet(AuthorizationRequestByReference authRequestByReference)
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Clear();
        
        return FromStr(await httpClient.GetStringAsync(authRequestByReference.RequestUri), Option<string>.None);
    }
    
    private async Task<Option<ClientMetadata>> FetchClientMetadata(AuthorizationRequest authorizationRequest)
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (authorizationRequest.ClientMetadata != null)
            return authorizationRequest.ClientMetadata;

        if (string.IsNullOrWhiteSpace(authorizationRequest.ClientMetadataUri))
            return null;
            
        var response = await httpClient.GetAsync(authorizationRequest.ClientMetadataUri);
        var clientMetadata = await response.Content.ReadAsStringAsync();
        
        return DeserializeObject<ClientMetadata>(clientMetadata);
    }
}
