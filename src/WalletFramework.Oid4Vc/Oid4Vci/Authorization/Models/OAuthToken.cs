using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;

/// <summary>
///     Represents a successful response from the OAuth 2.0 Authorization Server containing
///     the issued access token and related information.
/// </summary>
public class OAuthToken
{
    /// <summary>
    ///     Indicates if the Token Request is still pending as the Credential Issuer
    ///     is waiting for the End-User interaction to complete.
    /// </summary>
    [JsonProperty("authorization_pending")]
    public bool? AuthorizationPending { get; set; }

    /// <summary>
    ///     Gets or sets the lifetime in seconds of the c_nonce.
    /// </summary>
    [JsonProperty("c_nonce_expires_in")]
    public int? CNonceExpiresIn { get; set; }

    /// <summary>
    ///     Gets or sets the lifetime in seconds of the access token.
    /// </summary>
    [JsonProperty("expires_in")]
    public int? ExpiresIn { get; set; }

    /// <summary>
    ///     Gets or sets the minimum amount of time in seconds that the client should wait
    ///     between polling requests to the Token Endpoint.
    /// </summary>
    [JsonProperty("interval")]
    public int? Interval { get; set; }

    /// <summary>
    ///     Gets or sets the access token issued by the authorization server.
    /// </summary>
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    /// <summary>
    ///     Gets or sets the nonce to be used to create a proof of possession of key material
    ///     when requesting a Credential.
    /// </summary>
    [JsonProperty("c_nonce")]
    public string CNonce { get; set; }

    /// <summary>
    ///     Gets or sets the refresh token, which can be used to obtain new access tokens.
    /// </summary>
    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }

    /// <summary>
    ///     Gets or sets the scope of the access token.
    /// </summary>
    [JsonProperty("scope")]
    public string Scope { get; set; }

    /// <summary>
    ///     Gets or sets the type of the token issued.
    /// </summary>
    [JsonProperty("token_type")]
    public string TokenType { get; set; }
        
    /// <summary>
    ///     Gets or sets the credential identifier.
    /// </summary>
    [JsonProperty("credential_identifiers")]
    public AuthorizationDetails? CredentialIdentifier { get; set; }
}
