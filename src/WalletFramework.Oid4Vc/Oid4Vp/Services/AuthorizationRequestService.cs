using System.Net.Http.Headers;
using System.Web;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using static WalletFramework.Oid4Vc.Oid4Vp.Models.RequestObject;
using static WalletFramework.Oid4Vc.Oid4Vp.Models.ClientIdScheme.ClientIdSchemeValue;
using static Newtonsoft.Json.JsonConvert;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

public class AuthorizationRequestService(
    IHttpClientFactory httpClientFactory) : IAuthorizationRequestService
{
    public async Task<AuthorizationRequest> CreateAuthorizationRequest(AuthorizationRequestByReference authRequestByReference)
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Clear();
        
        var requestObjectJson = await httpClient.GetStringAsync(authRequestByReference.RequestUri);
        var requestObject = CreateRequestObject(requestObjectJson);
                
        var clientMetadata = await FetchClientMetadata(requestObject.ToAuthorizationRequest());
                
        return requestObject.ClientIdScheme.Value switch
        {
            X509SanDns => requestObject
                .ValidateJwtSignature()
                .ValidateTrustChain()
                .ValidateSanName()
                .ToAuthorizationRequest()
                .WithX509(requestObject)
                .WithClientMetadata(clientMetadata),
            //TODO: Remove Redirect URi in the future (kept for now for compatibility)
            RedirectUri => requestObject
                .ToAuthorizationRequest()
                .WithClientMetadata(clientMetadata),
            Did => requestObject
                .ToAuthorizationRequest()
                .WithClientMetadata(clientMetadata),
            VerifierAttestation =>
                throw new NotImplementedException("Verifier Attestation not yet implemented"),
            _ => throw new InvalidOperationException(
                $"Client ID Scheme {requestObject.ClientIdScheme} not supported")
        };
    }
    
    public async Task<AuthorizationRequest> CreateAuthorizationRequest(AuthorizationRequestByValue authRequestByValue)
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
        var authRequest =  AuthorizationRequest.CreateAuthorizationRequest(jsonString);

        var clientMetadata = await FetchClientMetadata(authRequest);
                
        return authRequest.ClientIdScheme.Value switch
        {
            RedirectUri => authRequest
                .WithClientMetadata(clientMetadata),
            _ => throw new InvalidOperationException(
                $"Client ID Scheme {authRequest.ClientIdScheme.Value} not supported when passing the Authorization Request within the Uri")
        };
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
