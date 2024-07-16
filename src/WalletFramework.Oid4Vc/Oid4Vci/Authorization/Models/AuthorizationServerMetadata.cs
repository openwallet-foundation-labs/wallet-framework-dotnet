using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;

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
    ///    Gets or sets the URL of the OAuth 2.0 JSON Web Key Set (JWKS) document.
    ///    Clients use this to verify the signatures from the Authorization Server. 
    /// </summary>
    [JsonProperty("jwks_uri")]
    public string JwksUri { get; set; }
        
    /// <summary>
    ///    Gets or sets the URL of the OAuth 2.0 authorization endpoint.
    /// </summary>
    [JsonProperty("authorization_endpoint")]
    public string AuthorizationEndpoint { get; set; }

    /// <summary>
    ///     Gets or sets the response types that the OAuth 2.0 Authorization Server supports.
    ///     These types determine how the Authorization Server responds to client requests.
    /// </summary>
    [JsonProperty("response_types_supported", NullValueHandling = NullValueHandling.Ignore)]
    public string[]? ResponseTypesSupported { get; set; }

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
        
    /// <summary>
    ///     Gets or sets the supported DPoP signing algorithms.
    ///     This indicates which algorithms the Authorization Server supports for DPoP Proof JWTs.
    /// </summary>
    [JsonProperty("dpop_signing_alg_values_supported")]
    public string[]? DPopSigningAlgValuesSupported { get; set; }
        
    /// <summary>
    ///     Gets or sets the URL of the endpoint where the wallet sends the Pushed Authorization Request (PAR) to.
    /// </summary>
    [JsonProperty("pushed_authorization_request_endpoint")]
    public string? PushedAuthorizationRequestEndpoint { get; set; }
        
    /// <summary>
    ///     Gets or sets a value indicating whether the Authorization Server requires the use of Pushed Authorization Requests.
    /// </summary>
    [JsonProperty("require_pushed_authorization_requests")]
    public bool? RequirePushedAuthorizationRequests { get; set; }
        
    internal bool IsDPoPSupported => DPopSigningAlgValuesSupported != null && DPopSigningAlgValuesSupported.Contains("ES256");
}
