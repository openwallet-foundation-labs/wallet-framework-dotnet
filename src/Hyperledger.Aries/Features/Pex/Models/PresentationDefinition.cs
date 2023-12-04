using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hyperledger.Aries.Features.Pex.Models
{
    /// <summary>
    /// Represents objects that articulate what proofs a Verifier requires
    /// </summary>
    public class PresentationDefinition
    {
        /// <summary>
        /// This MUST be a string. The string SHOULD provide a unique ID for the desired context.
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; private set; } = null!;

        /// <summary>
        /// Represents a collection of submission requirements
        /// </summary>
        [JsonProperty("submission_requirements")]
        public SubmissionRequirement[] SubmissionRequirements { get; private set; } = null!;
        
        /// <summary>
        /// Represents a collection of input descriptors.
        /// </summary>
        [JsonProperty("input_descriptors", Required = Required.Always)]
        public InputDescriptor[] InputDescriptors { get; private set; } = null!;
        
        /// <summary>
        /// This SHOULD be a human-friendly string intended to constitute a distinctive designation of the Presentation Definition.
        /// </summary>
        [JsonProperty("name")]
        public string? Name { get; private set; }

        /// <summary>
        /// This MUST be a string that describes the purpose for which the Presentation Definition's inputs are being used for.
        /// </summary>
        public string? Purpose { get; private set; }
        
        /// <summary>
        ///     Gets or sets the format of the presentation definition
        ///     This property is optional.
        /// </summary>
        [JsonProperty("format")]
        public Dictionary<string, Format> Formats { get; private set; } = null!;
    }
}
