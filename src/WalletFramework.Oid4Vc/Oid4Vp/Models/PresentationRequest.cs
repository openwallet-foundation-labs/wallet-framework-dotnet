using LanguageExt;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record PresentationRequest(
    AuthorizationRequest AuthorizationRequest,
    Option<List<PresentationCandidate>> Candidates)
{
    public Option<RpAuthResult> RpAuthResult => AuthorizationRequest.RpAuthResult;
}
