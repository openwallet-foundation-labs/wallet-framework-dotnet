using Newtonsoft.Json;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Authorization
{
    /// <summary>
    ///     Represents the metadata associated with an OAuth 2.0 Authorization Server.
    /// </summary>
    public class AuthorizationServerMetadata
    {
        /// <summary>
        ///     Gets or sets the issuer location for the OAuth 2.0 Authorization Server.
        /// </summary>
        [JsonProperty("issuer")]
        public string Issuer { get; set; }

        /// <summary>
        ///     Gets or sets the URL of the OAuth 2.0 token endpoint.
        ///     Clients use this endpoint to obtain an access token by presenting its authorization grant or refresh token.
        /// </summary>
        [JsonProperty("token_endpoint")]
        public string TokenEndpoint { get; set; }

        /// <summary>
        ///     Gets or sets the response types that the OAuth 2.0 Authorization Server supports.
        ///     These types determine how the Authorization Server responds to client requests.
        /// </summary>
        [JsonProperty("response_types_supported")]
        public string[] ResponseTypesSupported { get; set; }

        /// <summary>
        ///     Gets or sets the supported authentication methods the OAuth 2.0 Authorization Server supports
        ///     when calling the token endpoint.
        /// </summary>
        [JsonProperty("token_endpoint_auth_methods_supported")]
        public string[] TokenEndpointAuthMethodsSupported { get; set; }

        /// <summary>
        ///     Gets or sets the supported token endpoint authentication signing algorithms.
        ///     This indicates which algorithms the Authorization Server supports when receiving requests
        ///     at the token endpoint.
        /// </summary>
        [JsonProperty("token_endpoint_auth_signing_alg_values_supported")]
        public string[] TokenEndpointAuthSigningAlgValuesSupported { get; set; }
    }
}
