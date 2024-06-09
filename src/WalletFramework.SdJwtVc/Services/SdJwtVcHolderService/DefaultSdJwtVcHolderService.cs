using Hyperledger.Aries;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using Hyperledger.Indy.WalletApi;
using SD_JWT.Models;
using SD_JWT.Roles;
using WalletFramework.SdJwtVc.KeyStore.Services;
using WalletFramework.SdJwtVc.Models.Credential;
using WalletFramework.SdJwtVc.Models.Credential.Attributes;
using WalletFramework.SdJwtVc.Models.Issuer;
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

        /// <inheritdoc />
        public async Task<string> CreatePresentation(SdJwtRecord credential, string[] disclosedClaimPaths,
            string? audience = null,
            string? nonce = null)
        {
            var sdJwtDoc = credential.ToSdJwtDoc();
            var disclosures = new List<Disclosure>();
            foreach (var disclosure in sdJwtDoc.Disclosures)
            {
                if (disclosedClaimPaths.Any(disclosedClaim => disclosedClaim.StartsWith(disclosure.Path ?? string.Empty)))
                {
                    disclosures.Add(disclosure);
                }
            }
            
            var presentationFormat = Holder.CreatePresentationFormat(credential.EncodedIssuerSignedJwt, disclosures.ToArray());

            if (!string.IsNullOrEmpty(credential.KeyId) && !string.IsNullOrEmpty(nonce) &&
                !string.IsNullOrEmpty(audience))
            {
                var keybindingJwt =
                    await KeyStore.GenerateKbProofOfPossessionAsync(credential.KeyId, audience, nonce, "kb+jwt", presentationFormat.ToSdHash());
                return presentationFormat.AddKeyBindingJwt(keybindingJwt);
            }

            return presentationFormat.Value;
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
        [Obsolete("Use SaveAsync instead.")]
        public virtual async Task<string> StoreAsync(
            IAgentContext context, 
            string combinedIssuance,
            string keyId,
            IssuerMetadata issuerMetadata,
            List<CredentialDisplayMetadata> displayMetadata,
            Dictionary<string, ClaimMetadata> claimMetadata,
            Dictionary<string, string> issuerName
        )
        {
            var record = new SdJwtRecord(combinedIssuance, claimMetadata, displayMetadata, issuerName, keyId);

            await SaveAsync(context, record);
            return record.Id;
        }

        /// <inheritdoc />
        public virtual async Task SaveAsync(IAgentContext context, SdJwtRecord record)
        {
            try
            {
                await RecordService.AddAsync(context.Wallet, record);
            }
            catch (WalletItemAlreadyExistsException)
            {
                await RecordService.UpdateAsync(context.Wallet, record);
            }
        }
    }
    
    internal static class SdJwtRecordExtensions
    {
        internal static SdJwtDoc ToSdJwtDoc(this SdJwtRecord record)
        {
            return new SdJwtDoc(record.EncodedIssuerSignedJwt + "~" + string.Join("~", record.Disclosures) + "~");
        }
    }
}
