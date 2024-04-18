using System.Net;
using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services
{
    /// <inheritdoc />
    public class Oid4VpHaipClient : IOid4VpHaipClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPexService _pexService;

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

        /// <inheritdoc />
        public async Task<AuthorizationRequest> ProcessAuthorizationRequestAsync(HaipAuthorizationRequestUri haipAuthorizationRequestUri)
        {
            var authorizationRequest = new AuthorizationRequest();

            if (!String.IsNullOrEmpty(haipAuthorizationRequestUri.RequestUri))
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(haipAuthorizationRequestUri.RequestUri);
                
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    authorizationRequest = AuthorizationRequest.ParseFromJwt(content);
                }
            }
            else
            {
                authorizationRequest = AuthorizationRequest.ParseFromUri(haipAuthorizationRequestUri.Uri);
            }

            if (authorizationRequest == null) 
                throw new InvalidOperationException("Could not parse Authorization Request Url");
            
            if (!authorizationRequest.IsHaipConform())
                throw new InvalidOperationException("Authorization Request is not HAIP conform");

            return authorizationRequest;
        }
        
        /// <inheritdoc />
        public async Task<AuthorizationResponse> CreateAuthorizationResponseAsync(AuthorizationRequest authorizationRequest, (string inputDescriptorId, string presentation)[] presentationMap)
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

            var presentationSubmission = await _pexService.CreatePresentationSubmission(authorizationRequest.PresentationDefinition, descriptorMaps.ToArray());
            
            return new AuthorizationResponse
            {
                PresentationSubmission = JsonConvert.SerializeObject(presentationSubmission),
                VpToken = vpToken.Count > 1 ? JsonConvert.SerializeObject(vpToken) : vpToken[0],
                State = authorizationRequest.State
            };
        }
    }
}
