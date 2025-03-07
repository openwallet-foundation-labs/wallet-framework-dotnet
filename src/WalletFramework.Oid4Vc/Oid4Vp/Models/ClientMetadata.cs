using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

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
    public string? AuthorizationEncryptedResponseEnc { get; }
    
    /// <summary>
    ///    The redirect URIs of the client (verifier).
    /// </summary>
    [JsonProperty("redirect_uris")]
    public string[] RedirectUris { get; }
        
    /// <summary>
    ///    The name of the client (verifier).
    /// </summary>
    [JsonProperty("client_name")]
    public string? ClientName { get; }
    
    /// <summary>
    ///     The URI of a web page providing information about the client (verifier).
    /// </summary>
    [JsonProperty("client_uri")]
    public string? ClientUri { get; }
    
    /// <summary>
    ///    The ways to contact people responsible for this client (verifier).
    /// </summary>
    [JsonProperty("contacts")]
    public string[]? Contacts { get; }
        
    /// <summary>
    ///     The URI of the logo of the client (verifier).
    /// </summary>
    [JsonProperty("logo_uri")]
    public string? LogoUri { get; }
    
    [JsonProperty("jwks")]
    [JsonConverter(typeof(ClientJwksConverter))]
    [JsonIgnore]
    public List<JsonWebKey> Jwks { get; }
    
    /// <summary>
    ///     The URI to a human-readable privacy policy document for the client (verifier).
    /// </summary>
    [JsonProperty("policy_uri")]
    public string? PolicyUri { get; }
    
    /// <summary>
    ///     The URI to a human-readable terms of service document for the client (verifier).
    /// </summary>
    [JsonProperty("tos_uri")]
    public string? TosUri { get; }
    
    /// <summary>
    ///     The URI to a human-readable terms of service document for the client (verifier).
    /// </summary>
    [JsonProperty("vp_formats")]
    public Formats Formats { get; }

    public ClientMetadata(
        string? authorizationEncryptedResponseEnc,
        string? clientName,
        string? clientUri,
        string[]? contacts,
        string? logoUri,
        string? policyUri,
        string? tosUri,
        string[] redirectUris,
        List<JsonWebKey> jwks,
        Formats formats)
    {
        AuthorizationEncryptedResponseEnc = authorizationEncryptedResponseEnc;
        ClientName = clientName;
        ClientUri = clientUri;
        Contacts = contacts;
        LogoUri = logoUri;
        PolicyUri = policyUri;
        TosUri = tosUri;
        RedirectUris = redirectUris;
        Jwks = jwks;
        Formats = formats;
    }
}
