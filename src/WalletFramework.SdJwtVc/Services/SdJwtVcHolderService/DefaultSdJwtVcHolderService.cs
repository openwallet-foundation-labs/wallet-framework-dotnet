using Hyperledger.Aries;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using SD_JWT.Abstractions;
using SD_JWT.Models;
using WalletFramework.SdJwtVc.KeyStore.Services;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.SdJwtVc.Models.Records;

namespace WalletFramework.SdJwtVc.Services.SdJwtVcHolderService
{
    /// <inheritdoc />
    public class DefaultSdJwtVcHolderService : ISdJwtVcHolderService
    {
        /// <summary>
        ///     The service responsible for holder operations.
        /// </summary>
        protected readonly IHolder Holder;

        /// <summary>
        ///     The key store responsible for key operations.
        /// </summary>
        protected readonly IKeyStore KeyStore;

        /// <summary>
        ///     The service responsible for wallet record operations.
        /// </summary>
        protected readonly IWalletRecordService RecordService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultSdJwtVcHolderService" /> class.
        /// </summary>
        /// <param name="keyStore">The key store responsible for key operations.</param>
        /// <param name="recordService">The service responsible for wallet record operations.</param>
        /// <param name="holder">The service responsible for holder operations.</param>
        public DefaultSdJwtVcHolderService(
            IHolder holder,
            IKeyStore keyStore,
            IWalletRecordService recordService)
        {
            Holder = holder;
            KeyStore = keyStore;
            RecordService = recordService;
        }

        public async Task<SdJwtRecord[]> FindCredentialsByType(IAgentContext agentContext, string type)
        {
            var records = await ListAsync(agentContext);
            return records.Where(x => x.Vct == type).ToArray();
        }

        /// <inheritdoc />
        public async Task<string> CreatePresentation(SdJwtRecord credential, string[] disclosureNames,
            string? audience = null,
            string? nonce = null)
        {
            var disclosures = new List<Disclosure>();
            foreach (var disclosure in credential.Disclosures)
            {
                var deserializedDisclosure = Disclosure.Deserialize(disclosure);
                
                if (disclosureNames.Any(x => x == deserializedDisclosure.Name))
                {
                    disclosures.Add(deserializedDisclosure);
                }
            }

            string? keybindingJwt = null;
            if (!string.IsNullOrEmpty(credential.KeyId) && !string.IsNullOrEmpty(nonce) &&
                !string.IsNullOrEmpty(audience))
            {
                keybindingJwt =
                    await KeyStore.GenerateProofOfPossessionAsync(credential.KeyId, audience, nonce, "kb+jwt");
            }

            return Holder.CreatePresentation(credential.EncodedIssuerSignedJwt, disclosures.ToArray(), keybindingJwt);
        }

        /// <inheritdoc />
        public virtual async Task<bool> DeleteAsync(IAgentContext context, string recordId)
        {
            return await RecordService.DeleteAsync<SdJwtRecord>(context.Wallet, recordId);
        }

        

        /// <inheritdoc />
        public virtual async Task<SdJwtRecord> GetAsync(IAgentContext context, string credentialId)
        {
            var record = await RecordService.GetAsync<SdJwtRecord>(context.Wallet, credentialId);
            if (record == null)
                throw new AriesFrameworkException(ErrorCode.RecordNotFound, "SD-JWT Credential record not found");

            return record;
        }

        /// <inheritdoc />
        public virtual Task<List<SdJwtRecord>> ListAsync(IAgentContext context, ISearchQuery query = null,
            int count = 100,
            int skip = 0)
        {
            return RecordService.SearchAsync<SdJwtRecord>(context.Wallet, query, null, count, skip);
        }

        /// <inheritdoc />
        public virtual async Task<string> StoreAsync(
            IAgentContext context, 
            string combinedIssuance,
            string keyId,
            string issuerId,
            Dictionary<string, string> issuerDisplayInfo,
            List<CredentialDisplayInfo> credentialDisplayInfo,
            Dictionary<string, ClaimDisplayInfo> claimsDisplayInfo)
        {
            var sdJwtDoc = Holder.ReceiveCredential(combinedIssuance);
            var record = SdJwtRecord.FromSdJwtDoc(sdJwtDoc);

            record.SetDisplayInfo(credentialDisplayInfo, claimsDisplayInfo, issuerId, issuerDisplayInfo);
            
            record.Id = Guid.NewGuid().ToString();
            record.KeyId = keyId;
            await RecordService.AddAsync(context.Wallet, record);

            return record.Id;
        }
    }
}
