using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Query;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record CandidateQueryResult(
    Option<List<PresentationCandidateSet>> Candidates,
    Option<List<CredentialRequirement>> MissingCredentials);

public static class CandidateQueryResultFun
{
    public static Option<List<PresentationCandidate>> FlattenCandidates(this CandidateQueryResult result) =>
        result.Candidates.Map(candidates => candidates.SelectMany(c => c.Candidates).ToList());
}
