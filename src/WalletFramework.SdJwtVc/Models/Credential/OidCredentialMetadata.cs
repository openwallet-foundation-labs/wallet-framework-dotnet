#nullable enable

using Newtonsoft.Json;
using WalletFramework.SdJwtVc.Models.Credential.Attributes;

namespace WalletFramework.SdJwtVc.Models.Credential
{
    /// <summary>
    ///     Represents the metadata of a specific type of credential that a Credential Issuer can issue.
    /// </summary>
    public class OidCredentialMetadata
    {
        /// <summary>
        ///     Gets or sets the verifiable credential type (vct).
        /// </summary>
        [JsonProperty("vct")]
        public string Vct { get; set; } = null!;
        
        /// <summary>
        ///     Gets or sets the dictionary representing the attributes of the credential in different languages.
        /// </summary>
        [JsonProperty("claims")]
        public Dictionary<string, OidClaim>? Claims { get; set; }

        /// <summary>
        ///     Gets or sets a list of display properties of the supported credential for different languages.
        /// </summary>
        [JsonProperty("display", NullValueHandling = NullValueHandling.Ignore)]
        public List<OidCredentialDisplay>? Display { get; set; }

        /// <summary>
        ///     Gets or sets a list of methods that identify how the Credential is bound to the identifier of the End-User who
        ///     possesses the Credential.
        /// </summary>
        [JsonProperty("cryptographic_binding_methods_supported", NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? CryptographicBindingMethodsSupported { get; set; }

        /// <summary>
        ///     Gets or sets a list of identifiers for the signing algorithms that are supported by the issuer and used
        ///     to sign credentials.
        /// </summary>
        [JsonProperty("credential_signing_alg_values_supported", NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? CredentialSigningAlgValuesSupported { get; set; }

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
        
        /// <summary>
        ///     Gets or sets a dictionary which maps a credential type to its supported signing algorithms for key proofs.
        /// </summary>
        [JsonProperty("proof_types_supported", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, OidCredentialProofType>? ProofTypesSupported { get; set; }
    }
    
    /// <summary>
    ///     Represents credential type specific signing algorithm information.
    /// </summary>
    public class OidCredentialProofType
    {
        /// <summary>
        ///     Gets or sets the available signing algorithms for the associated credential type.
        /// </summary>
        [JsonProperty("proof_signing_alg_values_supported")]
        public string[] ProofSigningAlgValuesSupported { get; set; } = null!;
    }
}
