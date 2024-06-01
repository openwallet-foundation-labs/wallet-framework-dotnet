using Hyperledger.Aries.Agents;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization;
using WalletFramework.Oid4Vc.Oid4Vci.Models.CredentialOffer.GrantTypes;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata;

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
        /// <param name="context"></param>
        /// <param name="sessionId"></param>
        /// <param name="authorizationCodeParameters"></param>
        /// <param name="authorizationCode"></param>
        /// <param name="clientOptions"></param>
        /// <param name="metadataSet"></param>
        /// <param name="credentialConfigurationIds"></param>
        /// <returns></returns>
        Task<string> StoreAsync(
            IAgentContext context,
            VciSessionId sessionId,
            AuthorizationCodeParameters authorizationCodeParameters,
            AuthorizationCode authorizationCode,
            ClientOptions clientOptions,
            MetadataSet metadataSet,
            string[] credentialConfigurationIds);
        
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
