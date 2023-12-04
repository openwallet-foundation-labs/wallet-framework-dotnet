using System.Threading.Tasks;
using Hyperledger.Aries.Features.OpenId4Vc.Vp.Models;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vp.Services
{
    /// <summary>
    ///    This Service offers methods to handle the OpenId4Vp protocol
    /// </summary>
    internal interface IOid4VpHaipClient
    {
        /// <summary>
        ///     Processes an OpenID4VP Authorization Request Url.
        /// </summary>
        /// <param name="haipAuthorizationRequestUri"></param>
        /// <returns>
        ///     A task representing the asynchronous operation. The task result contains the Authorization Response object associated with the OpenID4VP Authorization Request Url.
        /// </returns>
        Task<AuthorizationRequest> ProcessAuthorizationRequestAsync(HaipAuthorizationRequestUri haipAuthorizationRequestUri);

        /// <summary>
        ///     Creates the Parameters that are necessary to send an OpenId4VP Authorization Response.
        /// </summary>
        /// <param name="authorizationRequest"></param>
        /// /// <param name="presentationMap"></param>
        /// <returns>
        ///     A task representing the asynchronous operation. The task result contains the Presentation Submission and the VP Token.
        /// </returns>
        Task<AuthorizationResponse> CreateAuthorizationResponseAsync(AuthorizationRequest authorizationRequest, (string inputDescriptorId, string presentation)[] presentationMap);
    }
}
