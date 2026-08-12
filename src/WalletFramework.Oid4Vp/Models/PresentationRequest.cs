using LanguageExt;
using WalletFramework.Oid4Vp.RelyingPartyAuthentication;

namespace WalletFramework.Oid4Vp.Models;

public record PresentationRequest(
    AuthorizationRequest AuthorizationRequest,
    CandidateQueryResult CandidateQueryResult)
{
    public Option<RpAuthResult> RpAuthResult => AuthorizationRequest.RpAuthResult;
}
