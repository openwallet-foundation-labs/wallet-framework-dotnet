using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

public class PresentationCandidateService(
    IPexService pexService,
    IDcqlService dcqlService) : IPresentationCandidateService
{
    public async Task<Option<IEnumerable<PresentationCandidate>>> FindPresentationCandidates(AuthorizationRequest authRequest)
    {
        if (authRequest.DcqlQuery is not null)
            return await dcqlService.FindCandidates(authRequest.DcqlQuery);

        if (authRequest.PresentationDefinition is not null)
            return await pexService.FindCandidates(authRequest);

        return Option<IEnumerable<PresentationCandidate>>.None;
    }
}
