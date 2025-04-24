using System.Net.Http.Headers;
using System.Web;
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
            async value =>
            {
                return await GetAuthRequestByValue(value);
            }
        );

    private async Task<Validation<AuthorizationRequestCancellation, RequestObject>> GetRequestObject(
        AuthorizationRequestByReference authRequestByReference)
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Clear();

        var jsonString = await httpClient.GetStringAsync(authRequestByReference.RequestUri);
        var requestObjectValidation = FromStr(jsonString);

        return await requestObjectValidation.MatchAsync(async requestObject =>
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
