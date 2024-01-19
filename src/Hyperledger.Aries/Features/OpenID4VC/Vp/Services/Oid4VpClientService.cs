using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.OpenId4Vc.Vp.Models;
using Hyperledger.Aries.Features.OpenId4Vc.Vp.Services;
using Hyperledger.Aries.Features.Pex.Models;
using Hyperledger.Aries.Features.SdJwt.Models.Records;
using Hyperledger.Aries.Features.SdJwt.Services.SdJwtVcHolderService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hyperledger.Aries.Features.OpenID4VC.Vp.Services
{
    /// <inheritdoc />
    internal class Oid4VpClientService : IOid4VpClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOid4VpHaipClient _oid4VpHaipClient;
        private readonly IOid4VpRecordService _oid4VpRecordService;
        private readonly ISdJwtVcHolderService _sdJwtVcHolderService;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="Oid4VpClientService" /> class.
        /// </summary>
        /// <param name="httpClientFactory">The http client factory to create http clients.</param>
        /// <param name="sdJwtVcHolderService">The service responsible for SD-JWT related operations.</param>
        /// <param name="oid4VpHaipClient">The service responsible for OpenId4VP related operations.</param>
        /// <param name="oid4VpRecordService">The service responsible for OidPresentationRecord related operations.</param>
        public Oid4VpClientService(
            IHttpClientFactory httpClientFactory,
            ISdJwtVcHolderService sdJwtVcHolderService,
            IOid4VpHaipClient oid4VpHaipClient,
            IOid4VpRecordService oid4VpRecordService)
        {
            _httpClientFactory = httpClientFactory;
            _sdJwtVcHolderService = sdJwtVcHolderService;
            _oid4VpHaipClient = oid4VpHaipClient;
            _oid4VpRecordService = oid4VpRecordService;
        }

        /// <inheritdoc />
        public async Task<(AuthorizationRequest, CredentialCandidates[])> ProcessAuthorizationRequestAsync(
            IAgentContext agentContext, Uri authorizationRequestUri)
        {
            var haipAuthorizationRequestUri = HaipAuthorizationRequestUri.FromUri(authorizationRequestUri);
            
            var authorizationRequest = await _oid4VpHaipClient.ProcessAuthorizationRequestAsync(haipAuthorizationRequestUri);

            var credentials = await _sdJwtVcHolderService.ListAsync(agentContext);
            var credentialCandidates = await _sdJwtVcHolderService.FindCredentialCandidates(credentials.ToArray(),
                authorizationRequest.PresentationDefinition.InputDescriptors);

            return (authorizationRequest, credentialCandidates);
        }

        /// <inheritdoc />
        public async Task<Uri?> PrepareAndSendAuthorizationResponseAsync(IAgentContext agentContext, AuthorizationRequest authorizationRequest, SelectedCredential[] selectedCredentials)
        {
            var authorizationResponse = await PrepareAuthorizationResponse(authorizationRequest, selectedCredentials);
            var redirectUri = await SendAuthorizationResponse(authorizationResponse, new Uri(authorizationRequest.ResponseUri));
            
            var presentedCredentials = 
                (from credential in selectedCredentials 
                    let inputD = authorizationRequest.PresentationDefinition.InputDescriptors
                        .FirstOrDefault(x => x.Id == credential.InputDescriptorId) 
                    select new PresentedCredential()
                    {
                        CredentialId = ((SdJwtRecord)credential.Credential).Id, 
                        PresentedClaims = GetPresentedClaimsForCredential(inputD, (SdJwtRecord)credential.Credential)
                    }).ToArray();
            
            await _oid4VpRecordService.StoreAsync(agentContext, authorizationRequest.ClientId,
                authorizationRequest.ClientMetadata, presentedCredentials);

            return redirectUri;
        }
        
        private async Task<AuthorizationResponse> PrepareAuthorizationResponse(AuthorizationRequest authorizationRequest, SelectedCredential[] selectedCredentials)
        {
            var presentationMaps =
                from credential in selectedCredentials.ToObservable()
                from inputDescriptor in authorizationRequest.PresentationDefinition.InputDescriptors
                where credential.InputDescriptorId == inputDescriptor.Id
                from presentation in Observable.FromAsync(async () => await CreatePresentation(
                    inputDescriptor,
                    (SdJwtRecord)credential.Credential,
                    authorizationRequest.ClientId,
                    authorizationRequest.Nonce))
                select (inputDescriptor.Id, presentation);

            return await _oid4VpHaipClient.CreateAuthorizationResponseAsync(authorizationRequest,
                await presentationMaps.ToArray());
        }

        private async Task<Uri?> SendAuthorizationResponse(AuthorizationResponse authorizationResponse, Uri responseUri)
        {
            var authorizationResponseJson = JsonConvert.SerializeObject(authorizationResponse);
            var authorizationResponseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(authorizationResponseJson);
            var requestContent = new List<KeyValuePair<string, string>>(authorizationResponseDict);

            var request = new HttpRequestMessage
            {
                RequestUri = responseUri,
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(requestContent)
            };

            var httpClient = _httpClientFactory.CreateClient();
            var responseMessage = await httpClient.SendAsync(request);

            if (!responseMessage.IsSuccessStatusCode)
                throw new InvalidOperationException("Authorization Response could not be sent");
            
            var content = await responseMessage.Content.ReadAsStringAsync();
            var redirectUri = string.IsNullOrEmpty(content) ? null : JObject.Parse(content)["redirect_uri"]?.ToString();
            return redirectUri == null ? null : new Uri(redirectUri);
        }

        private async Task<string> CreatePresentation(InputDescriptor inputDescriptor, SdJwtRecord credential,
            string clientId, string nonce)
        {
            var claimsToDisclose = GetDisclosureNamesFromInputDescriptor(inputDescriptor);
            return await _sdJwtVcHolderService.CreatePresentation(credential, claimsToDisclose, clientId, nonce);
        }

        private static string[]? GetDisclosureNamesFromInputDescriptor(InputDescriptor inputDescriptor)
            => inputDescriptor.Constraints.Fields == null
                ? null
                : (from field in inputDescriptor.Constraints.Fields
                    from path in field.Path
                    select path.Split(".").Last()).ToArray();

        private static Dictionary<string, PresentedClaim> GetPresentedClaimsForCredential(InputDescriptor inputDescriptor,
            SdJwtRecord sdJwtRecord)
            => (from field in inputDescriptor.Constraints.Fields 
                    from path in field.Path
                    select sdJwtRecord.Claims.FirstOrDefault(x => x.Key == path.Split(".").Last()))
                .ToDictionary(claim => claim.Key, claim => new PresentedClaim(){Value = claim.Value});
    }
}
