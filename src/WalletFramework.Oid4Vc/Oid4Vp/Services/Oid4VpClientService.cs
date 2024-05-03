using System.Reactive.Linq;
using Hyperledger.Aries.Agents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;
using static Newtonsoft.Json.JsonConvert;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services
{
    /// <inheritdoc />
    public class Oid4VpClientService : IOid4VpClientService
    {
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
            ILogger<Oid4VpClientService> logger,
            IOid4VpRecordService oid4VpRecordService)
        {
            _httpClientFactory = httpClientFactory;
            _sdJwtVcHolderService = sdJwtVcHolderService;
            _oid4VpHaipClient = oid4VpHaipClient;
            _logger = logger;
            _oid4VpRecordService = oid4VpRecordService;
        }

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOid4VpHaipClient _oid4VpHaipClient;
        private readonly ILogger<Oid4VpClientService> _logger;
        private readonly IOid4VpRecordService _oid4VpRecordService;
        private readonly ISdJwtVcHolderService _sdJwtVcHolderService;

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
        public async Task<Uri?> SendAuthorizationResponseAsync(
            IAgentContext agentContext,
            AuthorizationRequest authorizationRequest,
            SelectedCredential[] selectedCredentials)
        {
            var createPresentationMaps =
                from credential in selectedCredentials
                from inputDescriptor in authorizationRequest.PresentationDefinition.InputDescriptors
                where credential.InputDescriptorId == inputDescriptor.Id
                let disclosureNames = inputDescriptor
                    .Constraints
                    .Fields?
                    .SelectMany(field => field
                        .Path
                        .Select(path => path.Split(".").Last())
                    )
                let createPresentation = _sdJwtVcHolderService.CreatePresentation(
                    (SdJwtRecord)credential.Credential,
                    disclosureNames.ToArray(),
                    authorizationRequest.ClientId,
                    authorizationRequest.Nonce
                )
                select (inputDescriptor.Id, createPresentation);

            var presentationMaps = new List<(string, string)>();
            foreach (var (inputDescriptorId, createPresentation) in createPresentationMaps)
            {
                presentationMaps.Add((inputDescriptorId, await createPresentation));
            }

            var authorizationResponse = await _oid4VpHaipClient.CreateAuthorizationResponseAsync(
                authorizationRequest,
                presentationMaps.ToArray()
            );

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Clear();
            
            var responseMessage =
                await httpClient.SendAsync(
                    new HttpRequestMessage
                    {
                        RequestUri = new Uri(authorizationRequest.ResponseUri),
                        Method = HttpMethod.Post,
                        Content = new FormUrlEncodedContent(
                            DeserializeObject<Dictionary<string, string>>(
                                    SerializeObject(authorizationResponse)
                                )?
                                .ToList()
                            ?? throw new InvalidOperationException("Authorization Response could not be parsed")
                        )
                    }
                );

            if (!responseMessage.IsSuccessStatusCode)
                throw new InvalidOperationException("Authorization Response could not be sent");

            var presentedCredentials = selectedCredentials
                .Select(credential =>
                    {
                        var inputDescriptor =
                            authorizationRequest
                                .PresentationDefinition
                                .InputDescriptors
                                .Single(descriptor => descriptor.Id == credential.InputDescriptorId);

                        return new PresentedCredential
                        {
                            CredentialId = ((SdJwtRecord)credential.Credential).Id,
                            PresentedClaims =
                                inputDescriptor
                                    .Constraints
                                    .Fields?
                                    .SelectMany(
                                        field => field.Path.Select(
                                            path =>
                                                ((SdJwtRecord)credential.Credential)
                                                .Claims
                                                .First(x =>
                                                    x.Key == path.Split(".").Last()
                                                )
                                        )
                                    )
                                    .ToDictionary(
                                        claim => claim.Key,
                                        claim => new PresentedClaim { Value = claim.Value }
                                    )!
                        };
                    }
                );

            await _oid4VpRecordService.StoreAsync(
                agentContext,
                authorizationRequest.ClientId,
                authorizationRequest.ClientMetadata,
                authorizationRequest.PresentationDefinition.Name,
                presentedCredentials.ToArray()
            );

            var redirectUriJson = await responseMessage.Content.ReadAsStringAsync();

            try
            {
                return DeserializeObject<AuthorizationResponseCallback>(redirectUriJson);

            }
            catch (Exception e)
            {
                _logger.LogWarning("Could not parse Redirect URI received from: {ResponseUri} due to exception: {Exception}", authorizationRequest.ResponseUri, e);
                return null;
            }
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
