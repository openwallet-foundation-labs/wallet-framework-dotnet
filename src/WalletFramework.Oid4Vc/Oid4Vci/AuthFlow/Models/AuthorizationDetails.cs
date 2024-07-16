using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

/// <summary>
///    Represents the authorization details.
/// </summary>
public record AuthorizationDetails
{
    /// <summary>
    ///    Gets the type of the credential.
    /// </summary>
    [JsonProperty("type")] 
    public string Type { get; } = "openid_credential";
        
    /// <summary>
    ///   Gets the format of the credential.
    /// </summary>
    [JsonProperty("format", NullValueHandling = NullValueHandling.Ignore)]
    public string? Format { get; }
        
    /// <summary>
    ///   Gets the verifiable credential type (vct).
    /// </summary>
    [JsonProperty("vct", NullValueHandling = NullValueHandling.Ignore)]
    public string? Vct { get; }
    
    [JsonProperty("doctype", NullValueHandling = NullValueHandling.Ignore)]
    public string? DocType { get; }
    
    /// <summary>
    ///  Gets the credential configuration id.
    /// </summary>
    [JsonProperty("credential_configuration_id", NullValueHandling = NullValueHandling.Ignore)]
    public string? CredentialConfigurationId { get; }
        
    [JsonProperty("locations", NullValueHandling = NullValueHandling.Ignore)]
    public string[]? Locations { get; }
        
    internal AuthorizationDetails(
        string? format, 
        string? vct, 
        string? credentialConfigurationId, 
        string[]? locations,
        string? docType)
    {
        if (!string.IsNullOrWhiteSpace(format) && !string.IsNullOrWhiteSpace(credentialConfigurationId))
        {
            throw new ArgumentException("Both format and credentialConfigurationId cannot be present at the same time.");
        }
            
        Format = format;
        Vct = vct;
        CredentialConfigurationId = credentialConfigurationId;
        Locations = locations;
        DocType = docType;
    }
}
