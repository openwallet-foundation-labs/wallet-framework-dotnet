using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;
using static WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models.CredentialClaimQueryFun;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

/// <summary>
/// The credential query claim.
/// </summary>
// TODO: Validate for duplicate claim queries and ignore them
public class ClaimQuery
{
    /// <summary>
    /// This MUST be a string identifying the particular claim.
    /// </summary>
    [JsonProperty(IdJsonKey)]
    public string? Id { get; set; }
    
    /// <summary>
    /// A collection of strings representing a claims path pointer that specifies the path to a claim.
    /// </summary>
    [JsonProperty(PathJsonKey)]
    public string[]? Path { get; set; }
    
    /// <summary>
    /// A collection of strings, integers or booleans values that specifies the expected values of the claim.
    /// </summary>
    [JsonProperty(ValuesJsonKey)]
    public string[]? Values { get; set; }
    
    /// <summary>
    /// For Registration Certificate use only: Since most credential formats are supporting selective disclosures, a relying party has to explain why a field needs to be requested.
    /// </summary>
    [JsonProperty(PurposeJsonKey)]
    private Purpose[]? Purpose { get; set; }
    
    /// <summary>
    /// For mDoc format (up to VP Draft 23) this MUST be a string that specifies the namespace of the data element within the mdoc. 
    /// </summary>
    [JsonProperty(NamespaceJsonKey)]
    public string? Namespace { get; set; }
    
    /// <summary>
    /// For mDoc format (up to VP Draft 23) this MUST be a string that specifies the data element identifier of the data element within the provided namespace in the mDoc.
    /// </summary>
    [JsonProperty(ClaimNameJsonKey)]
    public string? ClaimName { get; set; }

    public static Validation<ClaimQuery> FromJObject(JObject json)
    {
        var id = json.GetByKey(IdJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                if (string.IsNullOrWhiteSpace(value.Value?.ToString()))
                {
                    return new StringIsNullOrWhitespaceError<CredentialQuery>();
                }

                return ValidationFun.Valid(value.Value.ToString());
            })
            .ToOption();
        
        var path = json.GetByKey(PathJsonKey)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJValue()))
            .OnSuccess(array => array.Select(value =>
            {
                if (string.IsNullOrWhiteSpace(value.Value?.ToString()))
                {
                    return new StringIsNullOrWhitespaceError<CredentialQuery>();
                }

                return ValidationFun.Valid(value.Value.ToString());;
            }))
            .OnSuccess(array => array.TraverseAll(x => x))
            .ToOption();
        
        var values = json.GetByKey(ValuesJsonKey)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJValue()))
            .OnSuccess(array => array.Select(value =>
            {
                if (string.IsNullOrWhiteSpace(value.Value?.ToString()))
                {
                    return new StringIsNullOrWhitespaceError<CredentialQuery>();
                }

                return ValidationFun.Valid(value.Value.ToString());;
            }))
            .OnSuccess(array => array.TraverseAll(x => x))
            .ToOption();

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

        var @namespace = json.GetByKey(NamespaceJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                if (string.IsNullOrWhiteSpace(value.Value?.ToString()))
                {
                    return new StringIsNullOrWhitespaceError<CredentialQuery>();
                }

                return ValidationFun.Valid(value.Value.ToString());
            })
            .ToOption();

        var claimName = json.GetByKey(ClaimNameJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                if (string.IsNullOrWhiteSpace(value.Value?.ToString()))
                {
                    return new StringIsNullOrWhitespaceError<CredentialQuery>();
                }

                return ValidationFun.Valid(value.Value.ToString());
            })
            .ToOption();

        return ValidationFun.Valid(Create)
            .Apply(id)
            .Apply(path)
            .Apply(values)
            .Apply(purpose)
            .Apply(@namespace)
            .Apply(claimName);
    }
    
    private static ClaimQuery Create(
        Option<string> id,
        Option<IEnumerable<string>> path,
        Option<IEnumerable<string>> values,
        Option<IEnumerable<Purpose>> purpose,
        Option<string> @namespace,
        Option<string> claimName) => new()
    {
        Id = id.ToNullable(),
        Path = path.ToNullable()?.ToArray(),
        Values = values.ToNullable()?.ToArray(),
        Purpose = purpose.ToNullable()?.ToArray(),
        Namespace = @namespace.ToNullable(),
        ClaimName = claimName.ToNullable()
    };
}

public static class CredentialClaimQueryFun
{
    public const string IdJsonKey = "id";
    public const string PathJsonKey = "path";
    public const string ValuesJsonKey = "values";
    public const string PurposeJsonKey = "purpose";
    public const string NamespaceJsonKey = "namespace";
    public const string ClaimNameJsonKey = "claim_name";
}
