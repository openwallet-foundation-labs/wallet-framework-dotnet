using LanguageExt;
using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Jwk;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

/// <summary>
///    Represents the metadata of a client (verifier).
/// </summary>
public record ClientMetadata
{
    /// <summary>
    ///     Defined the encoding that should be used when an encrypted Auth Response is requested by the verifier.
    /// </summary>
    [JsonProperty("authorization_encrypted_response_enc")]
    public string? AuthorizationEncryptedResponseEnc { get; init; }

    /// <summary>
    ///    The redirect URIs of the client (verifier).
    /// </summary>
    [JsonProperty("redirect_uris")]
    public string[] RedirectUris { get; init; }

    /// <summary>
    ///    The name of the client (verifier).
    /// </summary>
    [JsonProperty("client_name")]
    public string? ClientName { get; init; }

    /// <summary>
    ///     The URI of a web page providing information about the client (verifier).
    /// </summary>
    [JsonProperty("client_uri")]
    public string? ClientUri { get; init; }

    /// <summary>
    ///    The ways to contact people responsible for this client (verifier).
    /// </summary>
    [JsonProperty("contacts")]
    public string[]? Contacts { get; init; }

    /// <summary>
    ///     The URI of the logo of the client (verifier).
    /// </summary>
    [JsonProperty("logo_uri")]
    public string? LogoUri { get; init; }

    [JsonProperty("jwks")]
    [JsonConverter(typeof(ClientJwksConverter))]
    [JsonIgnore]
    public Option<JwkSet> JwkSet { get; init; }

    [JsonProperty("jwks_uri")] 
    public string? JwksUri { get; init; }

    /// <summary>
    ///     The URI to a human-readable privacy policy document for the client (verifier).
    /// </summary>
    [JsonProperty("policy_uri")]
    public string? PolicyUri { get; init; }

    /// <summary>
    ///     The URI to a human-readable terms of service document for the client (verifier).
    /// </summary>
    [JsonProperty("tos_uri")]
    public string? TosUri { get; init; }

    /// <summary>
    ///     The URI to a human-readable terms of service document for the client (verifier).
    /// </summary>
    [JsonProperty("vp_formats")]
    public Formats Formats { get; init; }
}
