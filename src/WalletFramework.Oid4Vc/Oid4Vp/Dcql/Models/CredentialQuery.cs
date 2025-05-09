using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.MdocLib;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Implementations;
using static WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models.CredentialQueryFun;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.MdocVc;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

/// <summary>
///     The credential query.
/// </summary>
public class CredentialQuery
{
    /// <summary>
    ///     An object defining claims in the requested credential.
    /// </summary>
    [JsonProperty(ClaimsJsonKey)]
    public ClaimQuery[]? Claims { get; set; }

    /// <summary>
    ///     An object defining additional properties requested by the Verifier.
    /// </summary>
    [JsonProperty(MetaJsonKey)]
    public CredentialMetaQuery? Meta { get; set; }

    /// <summary>
    ///     This MUST be a string that specifies the format of the requested Verifiable Credential.
    /// </summary>
    [JsonProperty(FormatJsonKey)]
    public string Format { get; set; } = null!;

    /// <summary>
    ///     This MUST be a string identifying the Credential in the response.
    /// </summary>
    [JsonProperty(IdJsonKey)]
    public string Id { get; set; } = null!;

    /// <summary>
    ///     Represents a collection, where each value contains a collection of identifiers for elements in claims that
    ///     specifies which combinations of claims for the credential are requested.
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

                return ValidationFun.Valid(value.Value.ToString());
            });

        var format = json.GetByKey(FormatJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                if (string.IsNullOrWhiteSpace(value.Value?.ToString()))
                {
                    return new StringIsNullOrWhitespaceError<CredentialQuery>();
                }

                return ValidationFun.Valid(value.Value.ToString());
            });

        var meta = json.GetByKey(MetaJsonKey)
            .OnSuccess(token => token.ToJObject())
            .OnSuccess(CredentialMetaQuery.FromJObject);

        var claims = json.GetByKey(ClaimsJsonKey)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJObject().OnSuccess(ClaimQuery.FromJObject)))
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
        Option<IEnumerable<ClaimQuery>> claims,
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
    public const string ClaimSetsJsonKey = "claim_sets";
    public const string ClaimsJsonKey = "claims";
    public const string FormatJsonKey = "format";
    public const string IdJsonKey = "id";
    public const string MetaJsonKey = "meta";

    public static IEnumerable<string> GetRequestedAttributes(this CredentialQuery credentialQuery) =>
        credentialQuery.Format switch
        {
            Constants.SdJwtVcFormat or Constants.SdJwtDcFormat
                => credentialQuery.Claims?.Select(claim => string.Join('.', claim.Path.GetPathComponents().Select(c => c.AsKey() ?? c.AsIndex()?.ToString() ?? "*"))) ?? [],
            Constants.MdocFormat =>
                credentialQuery.Claims?.Select(claim =>
                {
                    var components = claim.Path.GetPathComponents().ToArray();
                    var nameSpace = components.Length > 0 ? components[0].AsKey() : claim.Namespace;
                    var claimName = components.Length > 1 ? components[1].AsKey() : claim.ClaimName;
                    return $"['{nameSpace}']['{claimName}']";
                }) ?? [],
            _ => []
        };

    public static Option<OneOf<Vct, DocType>> GetRequestedCredentialType(this CredentialQuery credentialQuery)
    {
        switch (credentialQuery.Format)
        {
            case Constants.SdJwtVcFormat:
            case Constants.SdJwtDcFormat:
                return credentialQuery.Meta?.Vcts?.Any() == true
                    ? Option<OneOf<Vct, DocType>>.Some(
                        Vct.ValidVct(credentialQuery.Meta!.Vcts!.First()).UnwrapOrThrow())
                    : Option<OneOf<Vct, DocType>>.None;
            case Constants.MdocFormat:
                return credentialQuery.Meta?.Doctype?.Any() == true
                    ? Option<OneOf<Vct, DocType>>.Some(Vct.ValidVct(credentialQuery.Meta!.Doctype).UnwrapOrThrow())
                    : Option<OneOf<Vct, DocType>>.None;
            default:
                return Option<OneOf<Vct, DocType>>.None;
        }
    }

    public static Option<PresentationCandidate> FindMatchingCandidate(this CredentialQuery credentialQuery, IEnumerable<SdJwtRecord> sdJwts)
    {
        var vctValues = credentialQuery.Meta?.Vcts?.ToList();
        var claims = credentialQuery.Claims;

        var matches =
            from record in sdJwts
            let vctMatches = vctValues == null || vctValues.Count == 0 || vctValues.Contains(record.Vct)
            where vctMatches && claims.AreFulfilledBy(record.ToSdJwtDoc())
            select record;

        var setCandidates =
            from grouped in matches.GroupBy(sdJwt => sdJwt.GetCredentialSetId())
            let candidate = new CredentialSetCandidate(grouped.Key, grouped)
            where candidate.Credentials.Any()
            select candidate;

        return setCandidates.Any()
            ? new PresentationCandidate(credentialQuery.Id, setCandidates)
            : Option<PresentationCandidate>.None;
    }

    public static Option<PresentationCandidate> FindMatchingCandidate(this CredentialQuery credentialQuery, IEnumerable<MdocRecord> mdocs)
    {
        var doctype = credentialQuery.Meta?.Doctype;
        var claims = credentialQuery.Claims;

        var matches =
            from record in mdocs
            where record.DocType == doctype && claims.AreFulfilledBy(record.Mdoc)
            select record;

        var setCandidates =
            from grouped in matches.GroupBy(mdoc => mdoc.GetCredentialSetId())
            let candidate = new CredentialSetCandidate(grouped.Key, grouped)
            where candidate.Credentials.Any()
            select candidate;

        return setCandidates.Any()
            ? new PresentationCandidate(credentialQuery.Id, setCandidates)
            : Option<PresentationCandidate>.None;
    }
}
