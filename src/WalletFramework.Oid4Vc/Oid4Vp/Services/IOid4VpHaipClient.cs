using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

/// <summary>
///    This Service offers methods to handle the OpenId4Vp protocol
/// </summary>
public interface IOid4VpHaipClient
{
    /// <summary>
    ///     Creates the Parameters that are necessary to send an OpenId4VP Authorization Response.
    /// </summary>
    /// <param name="authorizationRequest"></param>
    /// <param name="presentationMaps"></param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result contains the Presentation Submission and the VP Token.
    /// </returns>
    Task<AuthorizationResponse> CreateAuthorizationResponseAsync(AuthorizationRequest authorizationRequest, PresentationMap[] presentationMaps);
}
