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
    ///     The URI of the logo of the client (verifier).
    /// </summary>
    [JsonProperty("logo_uri")]
    public string? LogoUri { get; }
    
    // TODO: JWKs of the verifier
    // public List<JWK> PublicKeys { get; }

    [JsonConstructor]
    private ClientMetadata(string? clientName, string? logoUri, string[] redirectUris)
    {
        ClientName = clientName;
        LogoUri = logoUri;
        RedirectUris = redirectUris;
    }
}
