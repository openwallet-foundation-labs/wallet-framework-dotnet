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
        /// <param name="context">Agent Context</param>
        /// <param name="sessionId">Session Identifier of a Authorization Code Flow session</param>
        /// <param name="authorizationCodeParameters">Parameters required for the authorization during the VCI authorization code flow.</param>
        /// <param name="clientOptions">Options specified by the Client (Wallet)</param>
        /// <param name="metadataSet">Consists of Issuer and Credential Metadata</param>
        /// <param name="credentialConfigurationIds">Identifiers of the Credentials that will be requested</param>
        /// <param name="authorizationCode">Authorization Code from the Credential Offer. Only used within the Issuer Initiated Authorization Code Flow</param>
        /// <returns></returns>
        Task<string> StoreAsync(
            IAgentContext context,
            VciSessionId sessionId,
            AuthorizationCodeParameters authorizationCodeParameters,
            ClientOptions clientOptions,
            MetadataSet metadataSet,
            string[] credentialConfigurationIds,
            AuthorizationCode? authorizationCode);
        
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
