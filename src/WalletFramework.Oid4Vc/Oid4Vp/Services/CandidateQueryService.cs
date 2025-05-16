using LanguageExt;
using OneOf;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

public class CandidateQueryService(
    IPexService pexService,
    IDcqlService dcqlService) : ICandidateQueryService
{
    public async Task<CandidateQueryResult> Query(AuthorizationRequest authRequest) =>
        await authRequest.Requirements.Match(
            dcqlService.Query,
            presentationDefinition =>
            {
                return pexService.FindPresentationCandidatesAsync(
                    presentationDefinition,
                    authRequest.ClientMetadata?.Formats
                );
            }
        );

    public async Task<Option<PresentationCandidate>> Query(OneOf<CredentialQuery, InputDescriptor> credentialRequirement)
    {
        return await credentialRequirement.Match(
            dcqlService.QuerySingle,
            pexService.FindPresentationCandidateAsync);
    }
}
