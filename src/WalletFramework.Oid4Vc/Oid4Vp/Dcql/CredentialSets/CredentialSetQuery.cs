using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;
using static WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialSets.CredentialSetQueryConstants;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialSets;

/// <summary>
/// The credential set query.
/// </summary>
public record CredentialSetQuery
{
    /// <summary>
    /// Specifies the purpose of the query.
    /// </summary>
    [JsonProperty(PurposeJsonKey)]
    [JsonConverter(typeof(PurposeConverter))]
    public Purpose[]? Purpose { get; init; }

    /// <summary>
    /// Indicates whether this set of Credentials is required to satisfy the particular use case at the Verifier.
    /// </summary>
    [JsonProperty("required")]
    public bool Required { get; init; } = true;

    /// <summary>
    /// Represents a collection, where each value is a list of Credential query identifiers representing one set Credentials that satisfies the use case.
    /// </summary>
    [JsonProperty(OptionsJsonKey)]
    [JsonConverter(typeof(CredentialSetJsonConverter))]
    public List<CredentialSetOption> Options { get; private init; } = null!;

    public static Validation<CredentialSetQuery> FromJObject(JObject json)
    {
        var purpose = json.GetByKey(PurposeJsonKey)
            .OnSuccess(token =>
            {
                return token switch
                {
                    JArray => token.ToJArray()
                        .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJObject()))
                        .OnSuccess(array =>
                            array.Select(RelyingPartyAuthentication.RegistrationCertificate.Purpose.FromJObject))
                        .OnSuccess(array => array.TraverseAll(x => x)),
                    JValue => token.ToJValue()
                        .OnSuccess(RelyingPartyAuthentication.RegistrationCertificate.Purpose.FromJValue)
                        .OnSuccess(purpose => new List<Purpose> { purpose }.AsEnumerable()),
                    _ => new StringIsNullOrWhitespaceError<Purpose>()
                };
            }).ToOption();

        var required = json.GetByKey(RequiredJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(jValue =>
            {
                var value = jValue.Value?.ToString();
                var required = false;
                if (string.IsNullOrWhiteSpace(value) && !bool.TryParse(value, out required))
                {
                    return new StringIsNullOrWhitespaceError<CredentialSetQuery>();
                }

                return ValidationFun.Valid(required);
            })
            .ToOption();

        var optionsValidation =
            from jToken in json.GetByKey(OptionsJsonKey)
            from jArray in jToken.ToJArray()
            from options in jArray.TraverseAll(token =>
            {
                return
                    from array in token.ToJArray()
                    from option in CredentialSetOption.FromJArray(array)
                    select option;
            })
            select options;

        return ValidationFun.Valid(Create)
            .Apply(purpose)
            .Apply(required)
            .Apply(optionsValidation);
    }

    private static CredentialSetQuery Create(
        Option<IEnumerable<Purpose>> purpose,
        Option<bool> required,
        IEnumerable<CredentialSetOption> options)
    {
        return new CredentialSetQuery
        {
            Purpose = purpose.ToNullable()?.ToArray(),
            Required = required.ToNullable() ?? false,
            Options = options.ToList()
        };
    }
}

public static class CredentialSetQueryConstants
{
    public const string PurposeJsonKey = "purpose";
    public const string RequiredJsonKey = "required";
    public const string OptionsJsonKey = "options";
}
