using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.ClientAttestation;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

/// <summary>
///   This Service offers methods to handle the OpenId4Vp protocol according to the HAIP
/// </summary>
public interface IOid4VpClientService
{
    /// <summary>
    ///     Processes an OpenID4VP Authorization Request Url.
    /// </summary>
    /// <param name="authorizationRequestUrl"></param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result contains the Authorization Response object associated with the OpenID4VP Authorization Request Url and Credentials Candidates that can be used to answer the request.
    /// </returns>
    Task<(AuthorizationRequest authorizationRequest, IEnumerable<PresentationCandidates> presentationCandidates)> ProcessAuthorizationRequestAsync(Uri authorizationRequestUrl);

    /// <summary>
    ///     Prepares and sends an Authorization Response containing a Presentation Submission and the VP Token to the Redirect Uri.
    /// </summary>
    /// <param name="authorizationRequest"></param>
    /// <param name="selectedCredentials"></param>
    /// <param name="combinedWalletAttestation"></param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result contains the Callback Url of the Authorization Response if present.
    /// </returns>
    Task<Uri?> SendAuthorizationResponseAsync(
        AuthorizationRequest authorizationRequest,
        IEnumerable<SelectedCredential> selectedCredentials,
        CombinedWalletAttestation? combinedWalletAttestation = null);
    
    /// <summary>
    ///     Prepares and sends an Authorization Response containing a Presentation Submission with on demand credentials (C'') and the VP Token to the Redirect Uri.
    /// </summary>
    /// <param name="authorizationRequest"></param>
    /// <param name="selectedCredentials"></param>
    /// <param name="issuanceSession"></param>
    /// <param name="combinedWalletAttestation"></param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result contains the Callback Url of the Authorization Response if present.
    /// </returns>
    Task<Uri?> SendAuthorizationResponseAsync(
        AuthorizationRequest authorizationRequest,
        IEnumerable<SelectedCredential> selectedCredentials,
        IssuanceSession issuanceSession,
        CombinedWalletAttestation? combinedWalletAttestation = null);
}
