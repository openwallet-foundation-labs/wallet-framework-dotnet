using Hyperledger.Aries.Agents;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization;

namespace WalletFramework.Oid4Vc.Oid4Vci.Services
{
    /// <summary>
    ///    Service for managing authorization records. They are used during the VCI Authorization Code Flow to hold session relevant inforation.
    /// </summary>
    public interface ISessionRecordService
    {
        /// <summary>
        ///    Stores the authorization session record.
        /// </summary>
        /// <param name="agentContext">The Agent Context</param>
        /// <param name="sessionId">Session Identifier of a Authorization Code Flow session</param>
        /// <param name="authorizationData">Options specified by the Client (Wallet)</param>
        /// <param name="authorizationCodeParameters">Parameters required for the authorization during the VCI authorization code flow.</param>
        /// <returns></returns>
        Task<string> StoreAsync(
            IAgentContext agentContext,
            VciSessionId sessionId,
            AuthorizationData authorizationData,
            AuthorizationCodeParameters authorizationCodeParameters);
        
        /// <summary>
        ///    Retrieves the authorization session record by the session identifier.
        /// </summary>
        /// <param name="context">Agent Context</param>
        /// <param name="sessionId">Session Identifier of a Authorization Code Flow session</param>
        /// <returns></returns>
        Task<VciAuthorizationSessionRecord> GetAsync(IAgentContext context, VciSessionId sessionId);
        
        /// <summary>
        ///     Deletes the authorization session record by the session identifier.
        /// </summary>
        /// <param name="context">Agent Context</param>
        /// <param name="sessionId">Session Identifier of a Authorization Code Flow session</param>
        /// <returns></returns>
        Task<bool> DeleteAsync(IAgentContext context, VciSessionId sessionId);
    }
}
