using Newtonsoft.Json;
using WalletFramework.SdJwtVc.Models.Credential.Attributes;

namespace WalletFramework.SdJwtVc.Models.Credential
{
    /// <summary>
    ///    Represents the detailed description of the credential type.
    /// </summary>
    public class CredentialDefinition
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
        public Dictionary<string, ClaimMetadata>? Claims { get; set; }
    }
}
