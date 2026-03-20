namespace WalletFramework.Oid4Vp.Models;

public record PresentationCandidateSet(List<PresentationCandidate> Candidates, bool IsRequired = true);
