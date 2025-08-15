using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;
using static WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models.CredentialMetaFun;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

/// <summary>
/// The credential query meta.
/// </summary>

public class CredentialMetaQuery
{
    /// <summary>
    /// Specifies allowed values for the type of the requested Verifiable credential.
    /// </summary>
    [JsonProperty(VctValuesJsonKey)]
    public IEnumerable<string>? Vcts { get; set; }
    
    /// <summary>
    /// Specifies an allowed value for the doctype of the requested Verifiable credential.
    /// </summary>
    [JsonProperty(DoctypeValueJsonKey)]
    public string? Doctype { get; set; }
    
    public static Validation<CredentialMetaQuery> FromJObject(JObject json)
    {
        var vcts = json.GetByKey(VctValuesJsonKey)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(array => array.Select(jToken =>
            {
                if (string.IsNullOrWhiteSpace(jToken.ToString()))
                {
                    return new StringIsNullOrWhitespaceError<CredentialMetaQuery>();
                }

                return ValidationFun.Valid(jToken.ToString());
            }))
            .OnSuccess(array => array.TraverseAll(x => x))
            .ToOption();

        var doctype = json.GetByKey(DoctypeValueJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                if (string.IsNullOrWhiteSpace(value.Value?.ToString()))
                {
                    return new StringIsNullOrWhitespaceError<Purpose>();
                }

                return ValidationFun.Valid(value.Value.ToString());
            })
            .ToOption();

        if (vcts.IsSome == doctype.IsSome)
        {
            return new ObjectRequirementsAreNotMetError<CredentialMetaQuery>(
                "In the CredentialMetaQuery the 'vct_values' and 'doctype_value' must be mutually exclusive.");
        }
        
        return ValidationFun.Valid(Create)
            .Apply(vcts)
            .Apply(doctype);
    }

    private static CredentialMetaQuery Create(
        Option<IEnumerable<string>> vcts,
        Option<string> doctype) => new()
    {
        Vcts = vcts.ToNullable(),
        Doctype = doctype.ToNullable()
    };
}

public static class CredentialMetaFun
{
    public const string VctValuesJsonKey = "vct_values";
    public const string DoctypeValueJsonKey = "doctype_value";
}
