using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Credential.Attributes;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Credential
{
    /// <summary>
    ///    Represents the detailed description of the credential type.
    /// </summary>
    public class OidCredentialDefinition
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
    }
}
