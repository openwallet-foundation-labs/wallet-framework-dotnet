using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Hyperledger.Aries.Features.Pex.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Hyperledger.Aries.Features.OpenId4Vc.Vp.Models.ClientIdScheme;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vp.Models
{
    /// <summary>
    ///     Represents the Request of a Verifier to a Holder within the OpenId4VP specification.
    /// </summary>
    public record AuthorizationRequest
    {
        private const string DirectPost = "direct_post";

        private const string VpToken = "vp_token";

        /// <summary>
        ///     Gets the client id scheme.
        /// </summary>
        [JsonProperty("client_id_scheme")]
        public ClientIdScheme ClientIdScheme { get; }

        /// <summary>
        ///     Gets the presentation definition. Contains the claims that the Verifier wants to receive.
        /// </summary>
        [JsonProperty("presentation_definition")]
        public PresentationDefinition PresentationDefinition { get; }

        /// <summary>
        ///     Gets the client id. The Identifier of the Verifier.
        /// </summary>
        [JsonProperty("client_id")]
        public string ClientId { get; }

        /// <summary>
        ///     Gets the nonce. Random string for session binding.
        /// </summary>
        [JsonProperty("nonce")]
        public string Nonce { get; }

        /// <summary>
        ///     Gets the response mode. Determines where to send the Authorization Response to.
        /// </summary>
        [JsonProperty("response_uri")]
        public string ResponseUri { get; }

        /// <summary>
        ///     Gets the client metadata. Contains the Verifier metadata.
        /// </summary>
        [JsonProperty("client_metadata")]
        public ClientMetadata? ClientMetadata { get; init; }

        /// <summary>
        ///     Gets the client metadata uri. Can be used to retrieve the verifier metadata.
        /// </summary>
        [JsonProperty("client_metadata_uri")]
        public string? ClientMetadataUri { get; }

        /// <summary>
        ///     The scope of the request.
        /// </summary>
        [JsonProperty("scope")]
        public string? Scope { get; }

        /// <summary>
        ///     Gets the state.
        /// </summary>
        [JsonProperty("state")]
        public string? State { get; }

        /// <summary>
        ///     The X509 certificate of the verifier, this property is only set when ClientIDScheme is X509SanDNS.
        /// </summary>
        public X509Certificate2? X509Certificate { get; init; }

        /// <summary>
        ///     The trust chain of the verifier, this property is only set when ClientIDScheme is X509SanDNS.
        /// </summary>
        public X509Chain? X509TrustChain { get; init; }

        [JsonConstructor]
        private AuthorizationRequest(
            ClientIdScheme clientIdScheme,
            PresentationDefinition presentationDefinition,
            string clientId,
            string nonce,
            string responseUri,
            ClientMetadata? clientMetadata,
            string? clientMetadataUri,
            string? scope,
            string? state)
        {
            ClientId = clientId;
            ClientIdScheme = clientIdScheme;
            ClientMetadata = clientMetadata;
            ClientMetadataUri = clientMetadataUri;
            Nonce = nonce;
            PresentationDefinition = presentationDefinition;
            ResponseUri = responseUri;
            Scope = scope;
            State = state;
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="AuthorizationRequest" /> class.
        /// </summary>
        /// <param name="authorizationRequestJson">The json representation of the authorization request.</param>
        /// <returns>A new instance of the <see cref="AuthorizationRequest" /> class.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the request does not match the HAIP.</exception>
        public static AuthorizationRequest CreateAuthorizationRequest(string authorizationRequestJson)
            => CreateAuthorizationRequest(JObject.Parse(authorizationRequestJson));

        private static AuthorizationRequest CreateAuthorizationRequest(JObject authorizationRequestJson) =>
            IsHaipConform(authorizationRequestJson)
                ? authorizationRequestJson.ToObject<AuthorizationRequest>()
                  ?? throw new InvalidOperationException("Could not parse the Authorization Request")
                : throw new InvalidOperationException(
                    "Invalid Authorization Request. The request does not match the HAIP"
                );

        private static bool IsHaipConform(JObject authorizationRequestJson)
        {
            var responseType = authorizationRequestJson["response_type"]!.ToString();
            var responseUri = authorizationRequestJson["response_uri"]!.ToString();
            var responseMode = authorizationRequestJson["response_mode"]!.ToString();
            var redirectUri = authorizationRequestJson["redirect_uri"];
            var clientIdScheme = authorizationRequestJson["client_id_scheme"]!.ToString();
            var clientId = authorizationRequestJson["client_id"]!.ToString();

            return
                responseType == VpToken
                && responseMode == DirectPost
                && !string.IsNullOrEmpty(responseUri)
                && redirectUri is null
                && (clientIdScheme is X509SanDnsScheme or VerifierAttestationScheme
                    || (clientIdScheme is RedirectUriScheme && clientId == responseUri));
        }
    }

    internal static class AuthorizationRequestExtensions
    {
        internal static AuthorizationRequest WithX509(
            this AuthorizationRequest authorizationRequest,
            RequestObject requestObject)
        {
            var encodedCertificate = requestObject.GetLeafCertificate().GetEncoded();

            var certificates =
                requestObject
                    .GetCertificates()
                    .Select(x => x.GetEncoded())
                    .Select(x => new X509Certificate2(x));

            var trustChain = new X509Chain();
            foreach (var element in certificates)
            {
                trustChain.ChainPolicy.ExtraStore.Add(element);
            }

            return authorizationRequest with
            {
                X509Certificate = new X509Certificate2(encodedCertificate),
                X509TrustChain = trustChain
            };
        }
        
        internal static AuthorizationRequest WithClientMetadata(
            this AuthorizationRequest authorizationRequest,
            ClientMetadata? clientMetadata) 
            => authorizationRequest with { ClientMetadata = clientMetadata };
    }
}
