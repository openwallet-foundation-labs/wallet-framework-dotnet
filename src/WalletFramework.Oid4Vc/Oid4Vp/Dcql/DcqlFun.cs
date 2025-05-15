using LanguageExt;
using WalletFramework.Core.Functional;
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
        var matchingSets =
            from setQuery in credentialSetQueries
            from option in setQuery.Options
            let matchingIds = option.Ids.Select(id => id.AsString())
            let setCandidates = candidates.Where(c => matchingIds.Contains(c.Identifier)).ToList()
            where setCandidates.Count == option.Ids.Count
            select new PresentationCandidateSet(setCandidates, setQuery.Required);

        var sets = matchingSets.ToList();
        return new CandidateQueryResult(
            sets.Count > 0 ? sets : Option<List<PresentationCandidateSet>>.None,
            missing.Count > 0 ? missing : Option<List<CredentialRequirement>>.None
        );
    }
}
