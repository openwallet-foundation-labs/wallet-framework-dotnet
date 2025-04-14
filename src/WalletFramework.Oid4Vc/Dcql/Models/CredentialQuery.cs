using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using static WalletFramework.Oid4Vc.Dcql.Models.CredentialQueryFun;

namespace WalletFramework.Oid4Vc.Dcql.Models;

/// <summary>
/// The credential query.
/// </summary>
public class CredentialQuery
{
    /// <summary>
    /// This MUST be a string identifying the Credential in the response.
    /// </summary>
    [JsonProperty(IdJsonKey)]
    public string Id { get; set; } = null!;

    /// <summary>
    /// This MUST be a string that specifies the format of the requested Verifiable Credential.
    /// </summary>
    [JsonProperty(FormatJsonKey)]
    public string Format { get; set; } = null!;

    /// <summary>
    /// An object defining additional properties requested by the Verifier.
    /// </summary>
    [JsonProperty(MetaJsonKey)]
    public CredentialMetaQuery? Meta { get; set; }
    
    /// <summary>
    /// An object defining claims in the requested credential.
    /// </summary>
    [JsonProperty(ClaimsJsonKey)]
    public CredentialClaimQuery[]? Claims { get; set; }
    
    /// <summary>
    /// Represents a collection, where each value contains a collection of identifiers for elements in claims that specifies which combinations of claims for the credential are requested.
    /// </summary>
    [JsonProperty(ClaimSetsJsonKey)]
    public string[][]? ClaimSets { get; set; }
    
    public static Validation<CredentialQuery> FromJObject(JObject json)
    {
        var id = json.GetByKey(IdJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                if (string.IsNullOrWhiteSpace(value.Value?.ToString()))
                {
                    return new StringIsNullOrWhitespaceError<CredentialQuery>();
                }

                return ValidationFun.Valid(value.Value.ToString());;
            });

        var format = json.GetByKey(FormatJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                if (string.IsNullOrWhiteSpace(value.Value?.ToString()))
                {
                    return new StringIsNullOrWhitespaceError<CredentialQuery>();
                }

                return ValidationFun.Valid(value.Value.ToString());;
            });

        var meta = json.GetByKey(MetaJsonKey)
            .OnSuccess(token => token.ToJObject())
            .OnSuccess(CredentialMetaQuery.FromJObject);
        
        var claims = json.GetByKey(ClaimsJsonKey)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJObject()))
            .OnSuccess(array => array.Select(CredentialClaimQuery.FromJObject))
            .OnSuccess(array => array.TraverseAll(x => x))
            .ToOption();

        var claimSets = json.GetByKey(ClaimSetsJsonKey)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJArray()))
            .OnSuccess(array => array.TraverseAll(innerArray =>
            {
                return innerArray.TraverseAll(x => x.ToJValue())
                    .OnSuccess(values => values.Select(value =>
                    {
                        if (string.IsNullOrWhiteSpace(value.Value?.ToString()))
                        {
                            return new StringIsNullOrWhitespaceError<CredentialQuery>();
                        }

                        return ValidationFun.Valid(value.Value.ToString());
                    }))
                    .OnSuccess(values => values.TraverseAll(x => x));
            }))
            .ToOption();

        return ValidationFun.Valid(Create)
            .Apply(id)
            .Apply(format)
            .Apply(meta)
            .Apply(claims)
            .Apply(claimSets);
    }

    private static CredentialQuery Create(
        string id,
        string format,
        CredentialMetaQuery meta,
        Option<IEnumerable<CredentialClaimQuery>> claims,
        Option<IEnumerable<IEnumerable<string>>> claimSets) => new()
    {
        Id = id,
        Format = format,
        Meta = meta,
        Claims = claims.ToNullable()?.ToArray(),
        ClaimSets = claimSets.ToNullable()?.Select(x => x.ToArray()).ToArray()
    };
}

public static class CredentialQueryFun
{
    public const string IdJsonKey = "id";
    public const string FormatJsonKey = "format";
    public const string MetaJsonKey = "meta";
    public const string ClaimsJsonKey = "claims";
    public const string ClaimSetsJsonKey = "claim_sets";
    
    public static IEnumerable<string> GetRequestedClaims(this CredentialQuery credentialQuery) =>
        credentialQuery.Format switch
        {
            Constants.SdJwtVcFormat or Constants.SdJwtDcFormat
                => credentialQuery.Claims?.Select(claim => string.Join('.', claim.Path)) ?? [],
            Constants.MdocFormat =>
                credentialQuery.Claims?.Select(claim =>
                {
                    // backward compatible Draft 24 & Draft 23
                    var nameSpace = claim.Path?[0] ?? claim.Namespace;
                    var claimName = claim.Path?[1] ?? claim.ClaimName;
                    return $"['{nameSpace}']['{claimName}']";
                }) ?? [],
            _ => []
        };
}
