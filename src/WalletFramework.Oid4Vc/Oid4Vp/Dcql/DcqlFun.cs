using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Query;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialSets;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql;

internal static class DcqlFun
{
    internal static CandidateQueryResult ProcessWith(
        this DcqlQuery query,
        IEnumerable<ICredential> credentials)
    {
        var pairs = query.CredentialQueries
            .Select(q => (Query: q, Candidate: q.FindMatchingCandidate(credentials)))
            .ToList();

        var candidates = pairs
            .Choose(x => x.Candidate)
            .ToList();

        var missing = pairs
            .Where(x => x.Candidate.IsNone)
            .Select(x => new CredentialRequirement(x.Query))
            .ToList();
        
        var setQueriesOption = query.CredentialSetQueries == null || query.CredentialSetQueries.Length == 0
            ? Option<CredentialSetQuery[]>.None
            : query.CredentialSetQueries;

        return setQueriesOption.Match(
            setQueries => BuildPresentationCandidateSets(setQueries, candidates, missing),
            () => {
                var singleSets = candidates.Count > 0
                    ? candidates.Select(c => new PresentationCandidateSet([c])).ToList()
                    : Option<List<PresentationCandidateSet>>.None;

                return new CandidateQueryResult(
                    singleSets,
                    missing.Count > 0 ? missing : Option<List<CredentialRequirement>>.None
                );
            }
        );
    }

    private static CandidateQueryResult BuildPresentationCandidateSets(
        CredentialSetQuery[] credentialSetQueries,
        IReadOnlyList<PresentationCandidate> candidates,
        List<CredentialRequirement> missing)
    {
        var sets = new List<PresentationCandidateSet>();
        foreach (var setQuery in credentialSetQueries)
        {
            var firstMatchingOption = setQuery.Options
                .Select(option =>
                {
                    return (
                        Option: option,
                        SetCandidates: candidates
                            .Where(c => option.Ids.Select(id => id.AsString()).Contains(c.Identifier))
                            .ToList()
                    );
                })
                .FirstOrDefault(x => x.SetCandidates.Count == x.Option.Ids.Count);

            if (firstMatchingOption != default)
            {
                sets.Add(new PresentationCandidateSet(firstMatchingOption.SetCandidates, setQuery.Required));
            }
        }
        return new CandidateQueryResult(
            sets.Count > 0 ? sets : Option<List<PresentationCandidateSet>>.None,
            missing.Count > 0 ? missing : Option<List<CredentialRequirement>>.None
        );
    }
}
