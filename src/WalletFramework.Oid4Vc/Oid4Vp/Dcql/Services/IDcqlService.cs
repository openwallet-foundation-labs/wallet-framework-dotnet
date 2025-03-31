using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;

public interface IDcqlService
{
    /// <summary>
    ///     Finds the presentation candidates based on the provided credentials and DCQL query.
    /// </summary>
    /// <returns>An array of presentation candidates, each containing a list of credentials that match the DCQL query.</returns>
    Task<Option<IEnumerable<PresentationCandidate>>> FindPresentationCandidatesAsync(DcqlQuery query);
    
    /// <summary>
    ///     Finds a presentation candidate based on the provided credentials and credential query.
    /// </summary>
    /// <returns>A presentation candidate that matches the DCQL query.</returns>
    Task<Option<PresentationCandidate>> FindPresentationCandidateAsync(CredentialQuery credentialQuery);
    
    /// <summary>
    ///     Creates the Parameters that are necessary to send an OpenId4VP Authorization Response.
    /// </summary>
    /// <param name="authorizationRequest"></param>
    /// <param name="presentationMaps"></param>
    /// <returns>
    ///     An authorization response including the VP Token.
    /// </returns>
    AuthorizationResponse CreateAuthorizationResponse(
        AuthorizationRequest authorizationRequest,
        PresentationMap[] presentationMaps);
}
