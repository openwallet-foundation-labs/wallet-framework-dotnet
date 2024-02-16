using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.OpenId4Vc.Vp.Models;
using Hyperledger.Aries.Features.OpenId4Vc.Vp.Services;
using Hyperledger.Aries.Features.SdJwt.Models.Records;
using Hyperledger.Aries.Features.SdJwt.Services.SdJwtVcHolderService;
using static Newtonsoft.Json.JsonConvert;
using static Newtonsoft.Json.Linq.JObject;

namespace Hyperledger.Aries.Features.OpenID4VC.Vp.Services
{
    /// <inheritdoc />
    internal class Oid4VpClientService : IOid4VpClientService
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
            IOid4VpRecordService oid4VpRecordService)
        {
            _httpClientFactory = httpClientFactory;
            _sdJwtVcHolderService = sdJwtVcHolderService;
            _oid4VpHaipClient = oid4VpHaipClient;
            _oid4VpRecordService = oid4VpRecordService;
        }

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOid4VpHaipClient _oid4VpHaipClient;
        private readonly IOid4VpRecordService _oid4VpRecordService;
        private readonly ISdJwtVcHolderService _sdJwtVcHolderService;

        /// <inheritdoc />
        public async Task<(AuthorizationRequest, CredentialCandidates[])> ProcessAuthorizationRequestAsync(
            IAgentContext agentContext, Uri authorizationRequestUri)
        {
            var haipAuthorizationRequestUri = HaipAuthorizationRequestUri.FromUri(authorizationRequestUri);

            var authorizationRequest =
                await _oid4VpHaipClient.ProcessAuthorizationRequestAsync(haipAuthorizationRequestUri);

            var credentials = await _sdJwtVcHolderService.ListAsync(agentContext);
            
            var credentialCandidates = await _sdJwtVcHolderService.FindCredentialCandidates(
                credentials.ToArray(),
                authorizationRequest.PresentationDefinition.InputDescriptors
            );

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

            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            var redirectUri = string.IsNullOrEmpty(responseContent)
                ? null
                : new Uri(Parse(responseContent)["redirect_uri"]?.ToString()!);

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

            return redirectUri;
        }
    }
}
