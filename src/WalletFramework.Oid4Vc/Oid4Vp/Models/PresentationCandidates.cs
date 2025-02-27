using LanguageExt;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record PresentationCandidates(
    AuthorizationRequest AuthorizationRequest,
    Option<List<PresentationCandidate>> Candidates);
