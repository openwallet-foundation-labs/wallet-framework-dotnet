using Hyperledger.Aries.Features.OpenId4Vc.Vp.Models;
using Hyperledger.Aries.Features.Pex.Models;
using Hyperledger.Aries.Features.Pex.Services;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Net.Http.Headers;
using static Newtonsoft.Json.JsonConvert;
using static Hyperledger.Aries.Features.OpenId4Vc.Vp.Models.RequestObject;
using static Hyperledger.Aries.Features.OpenId4Vc.Vp.Models.ClientIdScheme.ClientIdSchemeValue;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vp.Services
{
    /// <inheritdoc />
    internal class Oid4VpHaipClient : IOid4VpHaipClient
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Oid4VpHaipClient" /> class.
        /// </summary>
        /// <param name="httpClientFactory">The http client factory to create http clients.</param>
        /// <param name="pexService">The service responsible for presentation exchange protocol operations.</param>
        public Oid4VpHaipClient(
            IHttpClientFactory httpClientFactory,
            IPexService pexService)
        {
            _httpClientFactory = httpClientFactory;
            _pexService = pexService;
        }

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPexService _pexService;

        /// <inheritdoc />
        public async Task<AuthorizationResponse> CreateAuthorizationResponseAsync(
            AuthorizationRequest authorizationRequest,
            (string inputDescriptorId, string presentation)[] presentationMap)
        {
            var descriptorMaps = new List<DescriptorMap>();
            var vpToken = new List<string>();
            
            for (var index = 0; index < presentationMap.Length; index++)
            {
                vpToken.Add(presentationMap[index].presentation);

                var descriptorMap = new DescriptorMap
                {
                    Format = "vc+sd-jwt",
                    Path = presentationMap.Length > 1 ? "$[" + index + "]" : "$",
                    Id = presentationMap[index].inputDescriptorId,
                    PathNested = null
                };
                descriptorMaps.Add(descriptorMap);
            }

            var presentationSubmission =
                await _pexService.CreatePresentationSubmission(authorizationRequest.PresentationDefinition,
                    descriptorMaps.ToArray());

            return new AuthorizationResponse
            {
                PresentationSubmission = SerializeObject(presentationSubmission),
                VpToken = vpToken.Count > 1 ? SerializeObject(vpToken) : vpToken[0],
                State = authorizationRequest.State
            };
        }

        /// <inheritdoc />
        public async Task<AuthorizationRequest> ProcessAuthorizationRequestAsync(
            HaipAuthorizationRequestUri haipAuthorizationRequestUri)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Clear();
            
            var requestObject =
                CreateRequestObject(
                    await httpClient.GetStringAsync(haipAuthorizationRequestUri.RequestUri)
                );
            
            var clientMetadata = await FetchClientMetadata(requestObject.ToAuthorizationRequest());

            return
                requestObject.ClientIdScheme.Value switch
                {
                    X509SanDns =>
                        requestObject
                            .ValidateJwt()
                            .ValidateTrustChain()
                            .ValidateSanName()
                            .ToAuthorizationRequest()
                            .WithX509(requestObject)
                            .WithClientMetadata(clientMetadata),
                    RedirectUri =>
                        requestObject
                            .ToAuthorizationRequest()
                            .WithClientMetadata(clientMetadata),
                    VerifierAttestation =>
                        throw new NotImplementedException("Verifier Attestation not yet implemented"),
                    _ =>
                        throw new InvalidOperationException(
                            $"Client ID Scheme {requestObject.ClientIdScheme} not supported"
                        )
                };
        }
        
        private async Task<ClientMetadata?> FetchClientMetadata(AuthorizationRequest authorizationRequest)
        {
            var httpClient = _httpClientFactory.CreateClient();
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
}
