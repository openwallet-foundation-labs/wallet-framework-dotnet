using LanguageExt;
using OneOf;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

public interface ICandidateQueryService
{
    Task<Option<IEnumerable<PresentationCandidate>>> Query(AuthorizationRequest authRequest);

    Task<Option<PresentationCandidate>> Query(OneOf<CredentialQuery, InputDescriptor> credentialRequirement);
}
