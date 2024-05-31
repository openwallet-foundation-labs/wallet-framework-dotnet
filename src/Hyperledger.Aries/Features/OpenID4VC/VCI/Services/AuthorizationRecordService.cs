using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Authorization;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialOffer.GrantTypes;
using Hyperledger.Aries.Features.OpenID4VC.VCI.Models.Metadata;
using Hyperledger.Aries.Features.OpenID4VC.Vp.Services;
using Hyperledger.Aries.Storage;

namespace Hyperledger.Aries.Features.OpenID4VC.VCI.Services
{
    /// <inheritdoc />
    public class AuthorizationRecordService : IAuthorizationRecordService
    {
        /// <summary>
        ///     The service responsible for wallet record operations.
        /// </summary>
        protected readonly IWalletRecordService RecordService;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="Oid4VpRecordService" /> class.
        /// </summary>
        /// <param name="recordService">The service responsible for wallet record operations.</param>
        public AuthorizationRecordService(IWalletRecordService recordService)
        {
            RecordService = recordService;
        }
        
        /// <inheritdoc />
        public async Task<string> StoreAsync(
            IAgentContext agentContext,
            VciSessionId sessionId,
            AuthorizationCodeParameters authorizationCodeParameters,
            AuthorizationCode authorizationCode,
            ClientOptions clientOptions,
            MetadataSet metadataSet,
            string[] credentialConfigurationIds)
        {
            var record = new VciAuthorizationSessionRecord(
                sessionId, 
                authorizationCodeParameters, 
                authorizationCode, 
                clientOptions, 
                metadataSet, 
                credentialConfigurationIds);
            
            await RecordService.AddAsync(
                agentContext.Wallet,
                record
            );
            
            return record.Id;
        }
        
        /// <inheritdoc />
        public async Task<VciAuthorizationSessionRecord> GetAsync(IAgentContext context, VciSessionId sessionId)
        {
            var record = await RecordService.GetAsync<VciAuthorizationSessionRecord>(context.Wallet, sessionId);
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
