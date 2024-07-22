using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;

/// <summary>
///     Represents a Submission Requirement Object.
/// </summary>
public class SubmissionRequirement
{
    /// <summary>
    ///     Gets the count associated with this submission requirement. This indicates the number of Input Descriptors or
    ///     Submission Requirement Objects to be submitted.
    /// </summary>
    /// <remarks>
    ///     This is an optional property.
    /// </remarks>
    [JsonProperty("count")]
    public int? Count { get; private set; }

    /// <summary>
    ///     Gets the group string for this submission requirement.
    /// </summary>
    /// <remarks>
    ///     This property must contain a group string matching one of the group strings
    ///     specified for one or more Input Descriptor Objects.
    /// </remarks>
    [JsonProperty("from", Required = Required.Always)]
    public string From { get; private set; } = null!;

    /// <summary>
    ///     Gets the rule for this submission requirement.
    /// </summary>
    /// <remarks>
    ///     According to the HAIP, this property must be "pick".
    /// </remarks>
    [JsonProperty("rule", Required = Required.Always)]
    public string Rule { get; private set; } = null!;

    /// <summary>
    ///     Gets the name of the submission requirement.
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; private set; }

    /// <summary>
    ///     Private constructor for the SubmissionRequirement class.
    /// </summary>
    private SubmissionRequirement()
    {
    }

    /// <summary>
    ///     Gets or sets the from_nested property.
    /// </summary>
    /// <remarks>
    ///     According to the HAIP, this property must remain null because only "from" is supported, and you can set either
    ///     "from" or "from_nested," but not both.
    /// </remarks>
    [JsonProperty("from_nested")] private string? _fromNested;
}