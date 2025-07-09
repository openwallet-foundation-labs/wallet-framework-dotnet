using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.MdocLib;
using WalletFramework.Oid4Vc.Credential;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.SdJwtVc.Models;
using static WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries.CredentialQueryConstants;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;

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
    ///     This MUST be a CredentialQueryId identifying the Credential in the response.
    /// </summary>
    [JsonProperty(IdJsonKey)]
    [JsonConverter(typeof(CredentialQueryIdJsonConverter))]
    public CredentialQueryId Id { get; set; } = null!;

    /// <summary>
    ///     Represents a collection, where each value contains a collection of identifiers for elements in claims that
    ///     specifies which combinations of claims for the credential are requested.
    /// </summary>
    [JsonProperty(ClaimSetsJsonKey)]
    [JsonConverter(typeof(ClaimSetJsonConverter))]
    public IReadOnlyList<ClaimSet>? ClaimSets { get; set; }

    public static Validation<CredentialQuery> FromJObject(JObject json)
    {
        var id = json.GetByKey(IdJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value => CredentialQueryId.Create(value.Value?.ToString() ?? string.Empty))
            .ToOption();

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

        // TODO: claim sets must only be present if claims is present
        // TODO: what if the identifiers in claims and claim sets do not match?
        var claimSets =
            json.GetByKey(ClaimSetsJsonKey)
                .OnSuccess(token => token.ToJArray())
                .OnSuccess(ClaimSetFun.ValidateMany)
                .ToOption();

        return ValidationFun.Valid(Create)
            .Apply(id)
            .Apply(format)
            .Apply(meta)
            .Apply(claims)
            .Apply(claimSets);
    }

    private static CredentialQuery Create(
        Option<CredentialQueryId> id,
        string format,
        CredentialMetaQuery meta,
        Option<IEnumerable<ClaimQuery>> claims,
        Option<IEnumerable<ClaimSet>> claimSets) => new()
    {
        Id = id.ToNullable(),
        Format = format,
        Meta = meta,
        Claims = claims.ToNullable()?.ToArray(),
        ClaimSets = claimSets.ToNullable()?.ToArray()
    };
}

public static class CredentialQueryConstants
{
    public const string ClaimSetsJsonKey = "claim_sets";

    public const string ClaimsJsonKey = "claims";

    public const string FormatJsonKey = "format";

    public const string IdJsonKey = "id";

    public const string MetaJsonKey = "meta";
}

public static class CredentialQueryFun
{
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
                    ? Option<OneOf<Vct, DocType>>.Some(DocType.ValidDoctype(credentialQuery.Meta!.Doctype).UnwrapOrThrow())
                    : Option<OneOf<Vct, DocType>>.None;
            default:
                return Option<OneOf<Vct, DocType>>.None;
        }
    }

    public static Option<PresentationCandidate> FindMatchingCandidate(
        this CredentialQuery credentialQuery,
        IEnumerable<ICredential> credentials)
    {
        // Determine the credential types requested by the query
        var requestedTypes = credentialQuery.Meta?.Vcts?.Concat([credentialQuery.Meta.Doctype]).ToArray();

        // Filter credentials by requested types (if any)
        var credentialsWhereTypeMatches = credentials
            .Where(credential =>
            {
                return requestedTypes == null 
                    || !requestedTypes.Any()
                    || requestedTypes.Contains(credential.GetCredentialTypeAsString());
            })
            .ToArray();
        
        // Get the claims and claim sets to be disclosed
        var claims = credentialQuery.Claims ?? [];
        var claimSets = credentialQuery.ClaimSets ?? [];

        var toDisclose = claims.ProcessSets(claimSets).ToList();
        // Try to find and return a credentials that fulfill the claims
        foreach (var disclosures in toDisclose)
        {
            var matches = credentialsWhereTypeMatches
                .Where(disclosures.AreFulfilledBy)
                .ToArray();

            var groupedCandidates = matches
                .GroupBy(credential => credential.GetCredentialSetId())
                .Select(group => new CredentialSetCandidate(group.Key, group))
                .Where(candidate => candidate.Credentials.Any())
                .ToArray();

            if (groupedCandidates.Any())
            {
                return new PresentationCandidate(
                    credentialQuery.Id.AsString(),
                    groupedCandidates,
                    disclosures.ToList()
                );
            }
        }

        // If there are claims but not returned yet, that means there are claims who are not fulfilled
        if (toDisclose.Any())
            return Option<PresentationCandidate>.None;

        // If there are no claims asked, all credentials with the right type are valid
        var allCandidates = credentialsWhereTypeMatches
            .GroupBy(credential => credential.GetCredentialSetId())
            .Select(group => new CredentialSetCandidate(group.Key, group))
            .Where(candidate => candidate.Credentials.Any())
            .ToArray();

        return credentialsWhereTypeMatches.Any()
            ? new PresentationCandidate(credentialQuery.Id.AsString(), allCandidates, Option<List<ClaimQuery>>.None)
            : Option<PresentationCandidate>.None;
    }

    public static Option<IReadOnlyList<ClaimQuery>> GetClaimsToDisclose(this CredentialQuery credentialQuery)
    {
        if (credentialQuery.Claims == null)
        {
            return [];
        }

        // If claim_sets is absent, return all claims
        if (credentialQuery.ClaimSets == null)
        {
            return credentialQuery.Claims;
        }

        // If both claims and claim_sets are present, return the first claim_set that can be satisfied
        foreach (var claimSet in credentialQuery.ClaimSets)
        {
            var setClaims =
                (from claimId in claimSet.Claims
                 from claim in credentialQuery.Claims
                 where claim.Id != null && claim.Id.AsString() == claimId.AsString()
                 select claim).ToArray();

            // Only return if all claims in the set are present
            if (setClaims.Length == claimSet.Claims.Count)
                return setClaims;
        }

        return Option<IReadOnlyList<ClaimQuery>>.None;
    }

    public static IReadOnlyList<string> GetClaimsToDiscloseAsStrs(this CredentialQuery query) =>
        query.GetClaimsToDisclose().Match(claims => claims.AsStrings(query.Format), () => []);
}
