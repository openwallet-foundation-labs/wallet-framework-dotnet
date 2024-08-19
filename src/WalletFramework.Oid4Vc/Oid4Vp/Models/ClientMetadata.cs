using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

/// <summary>
///    Represents the metadata of a client (verifier).
/// </summary>
// TODO: Rename this to VerifierMetadata
public record ClientMetadata
{
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
    
    // TODO: JWKs of the verifier
    // public List<JWK> PublicKeys { get; }
    
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

    [JsonConstructor]
    private ClientMetadata(string? clientName, string? clientUri, string[]? contacts, string? logoUri, string? policyUri, string? tosUri, string[] redirectUris)
    {
        ClientName = clientName;
        ClientUri = clientUri;
        Contacts = contacts;
        LogoUri = logoUri;
        PolicyUri = policyUri;
        TosUri = tosUri;
        RedirectUris = redirectUris;
    }
}
