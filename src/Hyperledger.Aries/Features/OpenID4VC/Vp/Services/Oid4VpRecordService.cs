using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.OpenId4Vc.Vp.Models;
using Hyperledger.Aries.Storage;

namespace Hyperledger.Aries.Features.OpenID4VC.Vp.Services
{
    /// <inheritdoc />
    public class Oid4VpRecordService : IOid4VpRecordService
    {
        /// <summary>
        ///     The service responsible for wallet record operations.
        /// </summary>
        protected readonly IWalletRecordService RecordService;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="Oid4VpRecordService" /> class.
        /// </summary>
        /// <param name="recordService">The service responsible for wallet record operations.</param>
        public Oid4VpRecordService(IWalletRecordService recordService)
        {
            RecordService = recordService;
        }
        
        /// <inheritdoc />
        public async Task<OidPresentationRecord> GetAsync(IAgentContext context, string presentationId)
        {
            var record = await RecordService.GetAsync<OidPresentationRecord>(context.Wallet, presentationId);
            if (record == null)
                throw new AriesFrameworkException(ErrorCode.RecordNotFound, "OidPresentation record not found");

            return record;
        }

        /// <inheritdoc />
        public Task<List<OidPresentationRecord>> ListAsync(IAgentContext context, ISearchQuery? query = null, int count = 100, int skip = 0)
        {
            return RecordService.SearchAsync<OidPresentationRecord>(context.Wallet, query, null, count, skip);
        }

        /// <inheritdoc />
        public async Task<string> StoreAsync(
            IAgentContext context,
            string clientId,
            ClientMetadata? clientMetadata,
            string? name,
            PresentedCredential[] presentedCredentials)
        {
            var record = new OidPresentationRecord
            {
                ClientId = clientId,
                ClientMetadata = clientMetadata,
                Id = Guid.NewGuid().ToString(),
                PresentedCredentials = presentedCredentials,
                RecordVersion = 1
            };
            
            await RecordService.AddAsync(context.Wallet, record);
            
            return record.Id;
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(IAgentContext context, string recordId)
        {
            return RecordService.DeleteAsync<OidPresentationRecord>(context.Wallet, recordId);
        }
    }
}
