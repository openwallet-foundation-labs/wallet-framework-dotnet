using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

public interface ICandidateQueryService
{
    Task<CandidateQueryResult> Query(AuthorizationRequest authRequest);

    Task<Option<PresentationCandidate>> Query(CredentialQuery credentialRequirement);
}
