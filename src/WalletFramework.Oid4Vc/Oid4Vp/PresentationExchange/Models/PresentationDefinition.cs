using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;

/// <summary>
///     Represents objects that articulate what proofs a Verifier requires
/// </summary>
public record PresentationDefinition
{
    /// <summary>
    ///     Represents a collection of input descriptors.
    /// </summary>
    [JsonProperty("input_descriptors", Required = Required.Always)]
    public InputDescriptor[] InputDescriptors { get; init; }

    /// <summary>
    ///     This MUST be a string. The string SHOULD provide a unique ID for the desired context.
    /// </summary>
    [JsonProperty("id", Required = Required.Always)]
    public string Id { get; }

    /// <summary>
    ///     This SHOULD be a human-friendly string intended to constitute a distinctive designation of the Presentation
    ///     Definition.
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; }

    /// <summary>
    ///     This MUST be a string that describes the purpose for which the Presentation Definition's inputs are being used for.
    /// </summary>
    [JsonProperty("purpose")]
    public string? Purpose { get; }

    /// <summary>
    ///     Represents a collection of submission requirements
    /// </summary>
    [JsonProperty("submission_requirements")]
    public SubmissionRequirement[] SubmissionRequirements { get; }

    [JsonConstructor]
    private PresentationDefinition(
        InputDescriptor[] inputDescriptors,
        string id,
        string? name,
        string? purpose,
        SubmissionRequirement[] submissionRequirements)
    {
        InputDescriptors = inputDescriptors;
        Id = id;
        Name = name;
        Purpose = purpose;
        SubmissionRequirements = submissionRequirements;
    }
}
