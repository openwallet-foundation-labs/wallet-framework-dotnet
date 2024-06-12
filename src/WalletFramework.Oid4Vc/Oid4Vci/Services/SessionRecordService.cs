using Hyperledger.Aries;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization;
using WalletFramework.Oid4Vc.Oid4Vci.Models.CredentialOffer.GrantTypes;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata;
using WalletFramework.Oid4Vc.Oid4Vp.Services;

namespace WalletFramework.Oid4Vc.Oid4Vci.Services
{
    /// <inheritdoc />
    public class SessionRecordService : ISessionRecordService
    {
        /// <summary>
        ///     The service responsible for wallet record operations.
        /// </summary>
        protected readonly IWalletRecordService RecordService;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="Oid4VpRecordService" /> class.
        /// </summary>
        /// <param name="recordService">The service responsible for wallet record operations.</param>
        public SessionRecordService(IWalletRecordService recordService)
        {
            RecordService = recordService;
        }
        
        /// <inheritdoc />
        public async Task<string> StoreAsync(
            IAgentContext agentContext,
            VciSessionId sessionId,
            AuthorizationCodeParameters authorizationCodeParameters,
            ClientOptions clientOptions,
            MetadataSet metadataSet,
            string[] credentialConfigurationIds,
            AuthorizationCode? authorizationCode)
        {
            var record = new VciAuthorizationSessionRecord(
                sessionId, 
                authorizationCodeParameters,
                clientOptions, 
                metadataSet, 
                credentialConfigurationIds,
                authorizationCode);
            
            await RecordService.AddAsync(
                agentContext.Wallet,
                record
            );
            
            return record.Id;
        }
        
        /// <inheritdoc />
        public async Task<VciAuthorizationSessionRecord> GetAsync(IAgentContext context, VciSessionId sessionId)
        {
            var record = (await RecordService.SearchAsync<VciAuthorizationSessionRecord>(
                context.Wallet, 
                SearchQuery.Equal(
                    "~" + nameof(VciAuthorizationSessionRecord.SessionId),
                    sessionId
            ))).First();
            if (record == null)
                throw new AriesFrameworkException(ErrorCode.RecordNotFound, "VciAuthorizationSessionRecord record not found");

            return record;
        }
        
        /// <inheritdoc />
        public async Task<bool> DeleteAsync(IAgentContext context, VciSessionId sessionId)
        {
            return await RecordService.DeleteAsync<VciAuthorizationSessionRecord>(context.Wallet, sessionId);
        }
    }
}
