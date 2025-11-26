using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record CandidateQueryResult(
    Option<List<PresentationCandidateSet>> Candidates,
    Option<List<CredentialQuery>> MissingCredentials);

public static class CandidateQueryResultFun
{
    public static Option<List<PresentationCandidate>> FlattenCandidates(this CandidateQueryResult result) =>
        result.Candidates.Map(candidates => candidates.SelectMany(c => c.Candidates).ToList());
}
