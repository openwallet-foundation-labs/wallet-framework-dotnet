using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;
using static Newtonsoft.Json.JsonConvert;
using static WalletFramework.Oid4Vc.Oid4Vp.Models.RequestObject;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services
{
    /// <inheritdoc />
    public class Oid4VpHaipClient : IOid4VpHaipClient
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
                    Path = vpToken.Count > 1 ? "$[" + index + "]" : "$",
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

            return
                requestObject.ClientIdScheme.Value switch
                {
                    ClientIdScheme.ClientIdSchemeValue.X509SanDns =>
                        requestObject
                            .ValidateJwt()
                            .ValidateTrustChain()
                            .ValidateSanName()
                            .ToAuthorizationRequest()
                            .WithX509(requestObject),
                    ClientIdScheme.ClientIdSchemeValue.RedirectUri =>
                        requestObject.ToAuthorizationRequest(),
                    ClientIdScheme.ClientIdSchemeValue.VerifierAttestation =>
                        throw new NotImplementedException("Verifier Attestation not yet implemented"),
                    _ =>
                        throw new InvalidOperationException(
                            $"Client ID Scheme {requestObject.ClientIdScheme} not supported"
                        )
                };
        }
    }
}
