using Newtonsoft.Json;

namespace WalletFramework.SdJwtVc.Models;

/// <summary>
///     Represents the metadata of a specific type of credential that a Credential Issuer can issue.
/// </summary>
public class CredentialMetadataInfo
{
    /// <summary>
    ///     Gets or sets the credential definition which specifies a specific credential.
    /// </summary>
    [JsonProperty("credential_definition")]
    public CredentialDefinition CredentialDefinition { get; set; } = null!;

    /// <summary>
    ///     Gets or sets a list of display properties of the supported credential for different languages.
    /// </summary>
    [JsonProperty("display", NullValueHandling = NullValueHandling.Ignore)]
    public List<CredentialDisplayInfo>? Display { get; set; }

    /// <summary>
    ///     Gets or sets a list of methods that identify how the Credential is bound to the identifier of the End-User who
    ///     possesses the Credential.
    /// </summary>
    [JsonProperty("cryptographic_binding_methods_supported", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? CryptographicBindingMethodsSupported { get; set; }

    /// <summary>
    ///     Gets or sets a list of identifiers for the cryptographic suites that are supported.
    /// </summary>
    [JsonProperty("cryptographic_suites_supported", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? CryptographicSuitesSupported { get; set; }

    /// <summary>
    ///     A list of claim display names, arranged in the order in which they should be displayed by the Wallet.
    /// </summary>
    [JsonProperty("order", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? Order { get; set; }

    /// <summary>
    ///     Gets or sets the identifier for the format of the credential.
    /// </summary>
    [JsonProperty("format")]
    public string Format { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the unique identifier for the respective credential.
    /// </summary>
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string? Id { get; set; }
}

/// <summary>
///    Represents the detailed description of the credential type.
/// </summary>
public class CredentialDefinition
{
    /// <summary>
    ///     Gets or sets the verifiable credential type (vct).
    /// </summary>
    [JsonProperty("vct")]
    public string CredentialType { get; set; } = null!;
        
    /// <summary>
    ///     Gets or sets the dictionary representing the attributes of the credential in different languages.
    /// </summary>
    [JsonProperty("claims")]
    public Dictionary<string, ClaimDefinition>? Claims { get; set; }
}

/// <summary>
///     Represents the specifics about a claim.
/// </summary>
public class ClaimDefinition
{
    /// <summary>
    ///     Gets or sets the list of display properties associated with a specific credential attribute.
    /// </summary>
    /// <value>
    ///     The list of display properties. Each display property provides information on how the credential attribute should
    ///     be displayed.
    /// </value>
    [JsonProperty("display", NullValueHandling = NullValueHandling.Ignore)]
    public List<ClaimDisplayInfo>? Display { get; set; }

    /// <summary>
    ///     String value determining type of value of the claim. A non-exhaustive list of valid values defined by this
    ///     specification are string, number, and image media types such as image/jpeg.
    /// </summary>
    [JsonProperty("value_type", NullValueHandling = NullValueHandling.Ignore)]
    public string? ValueType { get; set; }
        
    /// <summary>
    ///     String value determining type of value of the claim. A non-exhaustive list of valid values defined by this
    ///     specification are string, number, and image media types such as image/jpeg.
    /// </summary>
    [JsonProperty("mandatory", NullValueHandling = NullValueHandling.Ignore)]
    public string? Mandatory { get; set; }
}
