using Newtonsoft.Json;

namespace WalletFramework.SdJwtVc.Models.Vct.Models
{
    /// <summary>
    ///     Represents the visual representations for the vc type.
    /// </summary>
    public class VctDisplay
    {
        /// <summary>
        ///     Gets or sets the human readable name for the type.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name { get; set; }
        
        /// <summary>
        ///     Gets or sets the human readable description for the type.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }
        
        /// <summary>
        ///     Gets or sets the rendering information for the type.
        /// </summary>
        // TODO: how to deal with different rendering models for rendering methods "simple" and "svg_template"?
        [JsonProperty("rendering", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, RenderingMetadata>? Rendering { get; set; }
    }
}
