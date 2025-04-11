using LanguageExt;
using OneOf;
using WalletFramework.Oid4Vc.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

public interface IPresentationCandidateService
{
    Task<Option<IEnumerable<PresentationCandidate>>> FindPresentationCandidatesAsync(AuthorizationRequest authRequest);
    
    Task<Option<PresentationCandidate>> FindPresentationCandidateAsync(OneOf<CredentialQuery, InputDescriptor> credentialRequirement);
}
