using System.Reactive.Linq;
using Hyperledger.Aries.Agents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services
{
    /// <inheritdoc />
    public class Oid4VpClientService : IOid4VpClientService
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
            
            var credentialCandidates = await FindCredentialCandidates(agentContext, 
                authorizationRequest.PresentationDefinition.InputDescriptors);

            return (authorizationRequest, credentialCandidates);
        }

        /// <inheritdoc />
        public async Task<Uri?> PrepareAndSendAuthorizationResponseAsync(
            IAgentContext agentContext,
            AuthorizationRequest authorizationRequest,
            SelectedCredential[] selectedCredentials)
        {
            var authorizationResponse = await PrepareAuthorizationResponse(
                authorizationRequest,
                selectedCredentials);
            
            var redirectUri = await SendAuthorizationResponse(
                authorizationResponse,
                new Uri(authorizationRequest.ResponseUri));
            
            var presentedCredentials = 
                from credential in selectedCredentials 
                let inputD = authorizationRequest
                    .PresentationDefinition
                    .InputDescriptors
                    .FirstOrDefault(x => x.Id == credential.InputDescriptorId) 
                select new PresentedCredential
                {
                    CredentialId = ((SdJwtRecord)credential.Credential).Id, 
                    PresentedClaims = GetPresentedClaimsForCredential(inputD, (SdJwtRecord)credential.Credential)
                };
            
            await _oid4VpRecordService.StoreAsync(
                agentContext,
                authorizationRequest.ClientId,
                authorizationRequest.ClientMetadata,
                authorizationRequest.PresentationDefinition.Name,
                presentedCredentials.ToArray());

            return redirectUri;
        }
        
        private async Task<AuthorizationResponse> PrepareAuthorizationResponse(AuthorizationRequest authorizationRequest, SelectedCredential[] selectedCredentials)
        {
            var presentationMaps =
                from credential in selectedCredentials.ToObservable()
                from inputDescriptor in authorizationRequest.PresentationDefinition.InputDescriptors
                where credential.InputDescriptorId == inputDescriptor.Id
                from presentation in Observable.FromAsync<string>(async () => await CreatePresentation(
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
        
        private async Task<CredentialCandidates[]> FindCredentialCandidates(IAgentContext agentContext, InputDescriptor[] inputDescriptors)
        {
            var result = new List<CredentialCandidates>();
        
            foreach (var inputDescriptor in inputDescriptors)
            {
                if (!inputDescriptor.Formats.Keys.Contains("vc+sd-jwt"))
                {
                    throw new NotSupportedException("Only vc+sd-jwt format is supported");
                }
        
                if (inputDescriptor.Constraints.Fields == null || inputDescriptor.Constraints.Fields.Length == 0)
                {
                    throw new InvalidOperationException("Fields cannot be null or empty");
                }

                var vct = (from field in inputDescriptor.Constraints.Fields
                                    where field.Path[0] == "$.vct"
                                    select field.Filter.Const).Single();   
                
                var credentials = await _sdJwtVcHolderService.FindCredentialsByType(agentContext, vct);
        
                var matchingCredentials =
                    FindMatchingCredentialsForFields(credentials, inputDescriptor.Constraints.Fields);
                if (matchingCredentials.Length == 0)
                {
                    continue;
                }
        
                var limitDisclosuresRequired = string.Equals(inputDescriptor.Constraints.LimitDisclosure, "required");
        
                var credentialCandidates = new CredentialCandidates(inputDescriptor.Id,
                    matchingCredentials, limitDisclosuresRequired);
        
                result.Add(credentialCandidates);
            }
        
            return result.ToArray();
        }
        
        private static SdJwtRecord[] FindMatchingCredentialsForFields(
            SdJwtRecord[] records, Field[] fields)
        {
            return (from sdJwtRecord in records
                let claimsJson = JsonConvert.SerializeObject(sdJwtRecord.Claims)
                let claimsJObject = JObject.Parse(claimsJson)
                let isFound =
                    (from field in fields
                        let candidate = claimsJObject.SelectToken(field.Path[0])
                        where candidate != null && (field.Filter == null ||
                                                    string.Equals(field.Filter.Const, candidate.ToString()))
                        select field).Count() == fields.Length
                where isFound
                select sdJwtRecord).ToArray();
        }
    }
}
