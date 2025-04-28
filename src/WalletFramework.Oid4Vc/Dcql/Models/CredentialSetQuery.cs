using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;
using static WalletFramework.Oid4Vc.Dcql.Models.CredentialSetQueryFun;

namespace WalletFramework.Oid4Vc.Dcql.Models;

/// <summary>
/// The credential set query.
/// </summary>
public class CredentialSetQuery
{
    /// <summary>
    /// Specifies the purpose of the query.
    /// </summary>
    [JsonProperty(PurposeJsonKey)]
    [JsonConverter(typeof(PurposeConverter))]
    public Purpose[]? Purpose { get; set; }
    
    /// <summary>
    /// Indicates whether this set of Credentials is required to satisfy the particular use case at the Verifier.
    /// </summary>
    [JsonProperty("required")]
    public bool Required { get; set; }

    /// <summary>
    /// Represents a collection, where each value is a list of Credential query identifiers representing one set Credentials that satisfies the use case.
    /// </summary>
    [JsonProperty(OptionsJsonKey)]
    public string[][]? Options { get; set; }
    
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

        var options = json.GetByKey(OptionsJsonKey)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJArray()))
            .OnSuccess(array => array.TraverseAll(innerArray =>
            {
                return innerArray.TraverseAll(x => x.ToJValue())
                    .OnSuccess(values => values.Select(value =>
                    {
                        if (string.IsNullOrWhiteSpace(value.Value?.ToString()))
                        {
                            return new StringIsNullOrWhitespaceError<CredentialSetQuery>();
                        }

                        return ValidationFun.Valid(value.Value.ToString());
                    }))
                    .OnSuccess(values => values.TraverseAll(x => x));
            }))
            .ToOption();

        return ValidationFun.Valid(Create)
            .Apply(purpose)
            .Apply(required)
            .Apply(options);
    }
    
    private static CredentialSetQuery Create(
        Option<IEnumerable<Purpose>> purpose,
        Option<bool> required,
        Option<IEnumerable<IEnumerable<string>>> options) => new()
    {
        Purpose = purpose.ToNullable()?.ToArray(),
        Required = required.ToNullable() ?? false,
        Options = options.ToNullable()?.Select(x => x.ToArray()).ToArray()
    };
}

public static class CredentialSetQueryFun
{
    public const string PurposeJsonKey = "purpose";
    public const string RequiredJsonKey = "required";
    public const string OptionsJsonKey = "options";
}
