using System;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.OpenId4Vc.Vp.Models;

namespace Hyperledger.Aries.Features.OpenID4VC.Vp.Services
{
    /// <summary>
    ///   This Service offers methods to handle the OpenId4Vp protocol according to the HAIP
    /// </summary>
    public interface IOid4VpClientService
    {
        /// <summary>
        ///     Processes an OpenID4VP Authorization Request Url.
        /// </summary>
        /// <param name="authorizationRequestUrl"></param>
        /// /// <param name="agentContext"></param>
        /// <returns>
        ///     A task representing the asynchronous operation. The task result contains the Authorization Response object associated with the OpenID4VP Authorization Request Url and Credentials Candidates that can be used to answer the request.
        /// </returns>
        Task<(AuthorizationRequest authorizationRequest, CredentialCandidates[] credentialCandidates)> ProcessAuthorizationRequestAsync(IAgentContext agentContext, Uri authorizationRequestUrl);

        /// <summary>
        ///     Prepares and sends an Authorization Response containing a Presentation Submission and the VP Token to the Redirect Uri.
        /// </summary>
        /// <param name="agentContext"></param>
        /// /// <param name="responseUri"></param>
        /// <param name="authorizationRequest"></param>
        /// <param name="selectedCredentials"></param>
        /// <returns>
        ///     A task representing the asynchronous operation. The task result contains the Callback Url of the Authorization Response if present.
        /// </returns>
        Task<Uri?> PrepareAndSendAuthorizationResponseAsync(IAgentContext agentContext, AuthorizationRequest authorizationRequest, SelectedCredential[] selectedCredentials);
    }
}
