using LanguageExt;
using OneOf;
using WalletFramework.Oid4Vc.Dcql.Models;
using WalletFramework.Oid4Vc.Dcql.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

public class PresentationCandidateService(
    IPexService pexService,
    IDcqlService dcqlService) : IPresentationCandidateService
{
    public async Task<Option<IEnumerable<PresentationCandidate>>> FindPresentationCandidatesAsync(AuthorizationRequest authRequest)
    {
        return await authRequest.Requirements.Match(
            dcqlService.FindPresentationCandidatesAsync,
            presentationDefinition => pexService.FindPresentationCandidatesAsync(presentationDefinition, authRequest.ClientMetadata?.Formats));
    }
    
    public async Task<Option<PresentationCandidate>> FindPresentationCandidateAsync(OneOf<CredentialQuery, InputDescriptor> credentialRequirement)
    {
        return await credentialRequirement.Match(
            dcqlService.FindPresentationCandidateAsync,
            pexService.FindPresentationCandidateAsync);
    }
}
