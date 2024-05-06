using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models
{
    /// <summary>
    /// Represents objects embedded within target Claim negotiation formats that express how the inputs presented as proofs to a Verifier are provided in accordance with the requirements specified in a Presentation Definition.
    /// </summary>
    public class PresentationSubmission
    {
        /// <summary>
        /// This MUST be a unique identifier, such as a UUID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; internal set; } = null!;
        
        /// <summary>
        /// This MUST be the id value of a valid Presentation Definition.
        /// </summary>
        [JsonProperty("definition_id")]
        public string DefinitionId { get; internal set; } = null!;
        
        /// <summary>
        /// This MUST be the id value of a valid Presentation Definition.
        /// </summary>
        [JsonProperty("descriptor_map")]
        public DescriptorMap[] DescriptorMap { get; internal set; } = null!;
    }
}
