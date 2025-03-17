using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

public interface IPresentationCandidateService
{
    Task<Option<IEnumerable<PresentationCandidate>>> FindPresentationCandidates(AuthorizationRequest authRequest);
}
