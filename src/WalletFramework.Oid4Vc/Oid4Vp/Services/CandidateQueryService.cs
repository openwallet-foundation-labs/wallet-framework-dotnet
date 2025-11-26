using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

public class CandidateQueryService(IDcqlService dcqlService) : ICandidateQueryService
{
    public async Task<CandidateQueryResult> Query(AuthorizationRequest authRequest) =>
        await dcqlService.Query(authRequest.DcqlQuery);

    public async Task<Option<PresentationCandidate>> Query(CredentialQuery credentialRequirement) =>
        await dcqlService.QuerySingle(credentialRequirement);
}
