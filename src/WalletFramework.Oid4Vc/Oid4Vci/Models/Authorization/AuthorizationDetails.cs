using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization
{
    /// <summary>
    ///    Represents the authorization details.
    /// </summary>
    internal record AuthorizationDetails
    {
        /// <summary>
        ///    Gets the type of the credential.
        /// </summary>
        [JsonProperty("type")] 
        public string Type { get; } = "openid_credential";
            
        /// <summary>
        ///  Gets or Sets the credential configuration id.
        /// </summary>
        [JsonProperty("credential_configuration_id")]
        public string CredentialConfigurationId { get; }
        
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("locations", NullValueHandling = NullValueHandling.Ignore)]
        public string[]? Locations { get; }
        
        internal AuthorizationDetails(
            string credentialConfigurationId, 
            string[]? locations)
        {
            if (string.IsNullOrWhiteSpace(credentialConfigurationId))
            {
                throw new ArgumentException("CredentialConfigurationId must be provided.");
            }
            
            CredentialConfigurationId = credentialConfigurationId;
            Locations = locations;
        }
    }
}
