using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;

/// <summary>
/// Pex Service.
/// </summary>
public interface IPexService
{
    /// <summary>
    ///     Finds the presentation candidates based on the provided credentials and input descriptors.
    /// </summary>
    /// <returns>An array of credential candidates, each containing a list of credentials that match the input descriptors.</returns>
    Task<Option<IEnumerable<PresentationCandidate>>> FindPresentationCandidatesAsync(PresentationDefinition presentationDefinition, Option<Formats> supportedFormatSigningAlgorithms);
    
    /// <summary>
    ///     Finds a presentation candidate based on the provided input descriptor.
    /// </summary>
    /// <returns>A presentation candidate that matches the input descriptor.</returns>
    Task<Option<PresentationCandidate>> FindPresentationCandidateAsync(InputDescriptor inputDescriptor);
    
    /// <summary>
    ///     Creates the Parameters that are necessary to send an OpenId4VP Authorization Response.
    /// </summary>
    /// <param name="authorizationRequest"></param>
    /// <param name="presentationMap"></param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result contains the Presentation Submission and the VP Token.
    /// </returns>
    Task<AuthorizationResponse> CreateAuthorizationResponseAsync(
        AuthorizationRequest authorizationRequest,
        (string Identifier, string Presentation, Format Format)[] presentationMap);
}
