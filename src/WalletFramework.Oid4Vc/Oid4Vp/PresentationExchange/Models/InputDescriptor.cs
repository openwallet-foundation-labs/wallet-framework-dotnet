using LanguageExt;
using Newtonsoft.Json;
using OneOf;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Qes.Authorization;
using WalletFramework.SdJwtVc.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;

/// <summary>
///     Represents details about an input descriptor. This class encapsulates properties for the top-level
///     of an Input Descriptor Object.
/// </summary>
public record InputDescriptor
{
    /// <summary>
    ///     Gets or sets the constraints for the input descriptor.
    ///     It defines conditions that must be met for the input.
    /// </summary>
    [JsonProperty("constraints", Required = Required.Always)]
    public Constraints Constraints { get; private set; } = null!;

    /// <summary>
    ///     Gets or sets the formats of the input descriptor.
    ///     This property is optional.
    /// </summary>
    [JsonProperty("format")]
    public Formats? Formats { get; private set; }

    [JsonIgnore]
    public Option<List<Uc5QesTransactionData>> TransactionData { get; init; }
        = Option<List<Uc5QesTransactionData>>.None;

    /// <summary>
    ///     Gets or sets the unique identifier for the input descriptor.
    ///     This MUST be a string that does not conflict with the id of another Input Descriptor Object
    ///     in the same Presentation Definition.
    /// </summary>
    [JsonProperty("id", Required = Required.Always)]
    public string Id { get; private set; } = null!;

    /// <summary>
    ///     Gets or sets the human-friendly name that describes what the target schema represents.
    ///     This property is optional.
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; private set; }

    /// <summary>
    ///     Gets or sets the purpose for which the Claim's data is being requested.
    ///     This property is optional.
    /// </summary>
    [JsonProperty("purpose")]
    public string? Purpose { get; private set; }

    /// <summary>
    ///     Gets the array of groups that the input descriptor belongs to. Needed for Submission Requirement Feature.
    /// </summary>
    [JsonProperty("group")]
    public string[]? Group { get; private set; }
}

/// <summary>
///     Represents constraints that are associated with an input descriptor.
///     Defines conditions that the input must meet.
/// </summary>
public class Constraints
{
    /// <summary>
    ///     Gets the array of fields.
    ///     This property is optional.
    /// </summary>
    [JsonProperty("fields")]
    public Field[]? Fields { get; private set; }

    /// <summary>
    ///     Gets the requirement for limit disclosures.
    ///     This property is optional.
    /// </summary>
    [JsonProperty("limit_disclosure")]
    public string? LimitDisclosure { get; private set; }
}

/// <summary>
///     Represents the detailed structure of a specific field within the constraints.
/// </summary>
public class Field
{
    /// <summary>
    ///     Gets the filter associated with the field to evaluate values against.
    /// </summary>
    [JsonProperty("filter")]
    public Filter? Filter { get; private set; }

    /// <summary>
    ///     Gets an array of JSONPath string expressions that select a target value from the input.
    /// </summary>
    [JsonProperty("path")]
    public string[] Path { get; private set; } = null!;
}

/// <summary>
///     Represents a filter applied to the value that is selected by the field's path.
/// </summary>
public class Filter
{
    /// <summary>
    ///     Gets the type of filter applied.
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; private set; } = null!;

    /// <summary>
    ///     Gets the constant value which the selected value is evaluated against.
    /// </summary>
    [JsonProperty("const")]
    public string? Const { get; private set; }

    /// <summary>
    ///     Gets the constant value which the selected value is evaluated against.
    /// </summary>
    [JsonProperty("enum")]
    public string[]? Enum { get; private set; }
}

public static class InputDescriptorFun
{
    public static IEnumerable<string> GetRequestedAttributes(this InputDescriptor inputDescriptor) =>
        inputDescriptor.Constraints.Fields?
            .SelectMany(field => field.Path)
            .Select(s => s.TrimStart('$', '.')) ?? [];

    public static Option<OneOf<Vct, DocType>> GetRequestedCredentialType(this InputDescriptor inputDescriptor)
    {
        List<OneOf<Vct, DocType>> result = [];

        if (inputDescriptor.Formats?.SdJwtVcFormat != null || inputDescriptor.Formats?.SdJwtDcFormat != null)
        {
            var types = inputDescriptor
                .Constraints
                .Fields?
                .Where(field => !string.IsNullOrWhiteSpace(field.Filter?.Const))
                .Select<Field, OneOf<Vct, DocType>>(field => Vct.ValidVct(field.Filter!.Const!).UnwrapOrThrow());

            result = types?.ToList() ?? [];
        }

        if (inputDescriptor.Formats?.MDocFormat != null)
        {
            var types = inputDescriptor
                .Constraints
                .Fields?
                .Where(field => !string.IsNullOrWhiteSpace(field.Filter?.Const))
                .Select<Field, OneOf<Vct, DocType>>(field =>
                {
                    return DocType.ValidDoctype(field.Filter!.Const!).UnwrapOrThrow();
                });

            result = types?.ToList() ?? [];
        }

        return result.Count != 0 ? result[0] : Option<OneOf<Vct, DocType>>.None;
    }
}
