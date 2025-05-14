using LanguageExt;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record PresentationRequest(
    AuthorizationRequest AuthorizationRequest,
    CandidateQueryResult CandidateQueryResult)
{
    public Option<RpAuthResult> RpAuthResult => AuthorizationRequest.RpAuthResult;
}
