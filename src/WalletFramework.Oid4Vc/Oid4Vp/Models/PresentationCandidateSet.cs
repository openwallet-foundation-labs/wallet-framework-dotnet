namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record PresentationCandidateSet(List<PresentationCandidate> Candidates, bool IsRequired = true);

public static class PresentationCandidateSetFun
{
    public static PresentationCandidateSet ToSet(this IEnumerable<PresentationCandidate> candidates)
    {
        return new PresentationCandidateSet([.. candidates]);
    }
}
