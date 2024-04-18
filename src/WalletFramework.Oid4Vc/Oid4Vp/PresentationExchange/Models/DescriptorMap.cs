using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models
{
    /// <summary>
    /// The descriptor.
    /// </summary>
    public class DescriptorMap
    {
        /// <summary>
        /// This MUST be a string that matches the id property of the Input Descriptor in the Presentation Definition that this Presentation Submission is related to.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = null!;

        /// <summary>
        /// This MUST be a string that matches one of the Claim Format Designation
        /// </summary>
        [JsonProperty("format")]
        public string Format { get; set; } = null!;

        /// <summary>
        /// This MUST be a JSONPath string expression.
        /// The path property indicates the Claim submitted in relation to the identified Input Descriptor, when executed against the top-level of the object the Presentation Submission is embedded within.
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; } = null!;
        
        /// <summary>
        /// This indicate the presence of a multi-Claim envelope format. This means the Claim indicated is to be decoded separately from its parent enclosure
        /// </summary>
        [JsonProperty("path_nested", NullValueHandling = NullValueHandling.Ignore)]
        public DescriptorMap? PathNested { get; set; }
    }
}
