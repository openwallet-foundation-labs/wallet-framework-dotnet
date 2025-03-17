using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;

public interface IDcqlService
{
    /// <summary>
    ///     Finds the presentation candidates based on the provided credentials and DCQL query.
    /// </summary>
    /// <returns>An array of presentation candidates, each containing a list of credentials that match the DCQL query.</returns>
    Task<Option<IEnumerable<PresentationCandidate>>> FindCandidates(DcqlQuery query);
    
    /// <summary>
    ///     Finds the credential candidates based on the provided credentials and credential query.
    /// </summary>
    /// <returns>A presentation candidate that matches the DCQL query.</returns>
    Task<Option<PresentationCandidate>> FindCandidates(CredentialQuery credentialQuery);
    
    /// <summary>
    ///     Creates the Parameters that are necessary to send an OpenId4VP Authorization Response.
    /// </summary>
    /// <param name="authorizationRequest"></param>
    /// <param name="presentationMap"></param>
    /// <returns>
    ///     An authorization response including the VP Token.
    /// </returns>
    AuthorizationResponse CreateAuthorizationResponse(
        AuthorizationRequest authorizationRequest,
        (string Identifier, string Presentation, Format Format)[] presentationMap);
}
