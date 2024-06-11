using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using WalletFramework.SdJwtVc.Models.Credential;
using WalletFramework.SdJwtVc.Models.Credential.Attributes;
using WalletFramework.SdJwtVc.Models.Issuer;
using WalletFramework.SdJwtVc.Models.Records;

namespace WalletFramework.SdJwtVc.Services.SdJwtVcHolderService
{
    /// <summary>
    ///     Provides methods for handling SD-JWT credentials.
    /// </summary>
    public interface ISdJwtVcHolderService
    {
        /// <summary>
        ///     Deletes a specific SD-JWT record by its ID.
        /// </summary>
        /// <param name="context">The agent context.</param>
        /// <param name="recordId">The ID of the SD-JWT credential record to delete.</param>
        /// <returns>
        ///     A task representing the asynchronous operation. The task result indicates whether the deletion was successful.
        /// </returns>
        Task<bool> DeleteAsync(IAgentContext context, string recordId);

        /// <summary>
        ///     Creates a SD-JWT in presentation format where the provided claims are disclosed.
        ///     The key binding is optional and can be activated by providing an audience and a nonce.
        /// </summary>
        /// <remarks>
        ///     The SD-JWT is created using the provided SD-JWT credential and the provided claims are disclosed
        /// </remarks>
        /// <param name="disclosedClaimPaths">The claims to disclose</param>
        /// <param name="credential">The SD-JWT credential</param>
        /// <param name="audience">The targeted audience</param>
        /// <param name="nonce">The nonce</param>
        /// <returns>The SD-JWT in presentation format</returns>
        Task<string> CreatePresentation(SdJwtRecord credential, string[] disclosedClaimPaths, string? audience = null, string? nonce = null);
        
        /// <summary>
        ///     Retrieves a specific SD-JWT record by its ID.
        /// </summary>
        /// <param name="context">The agent context.</param>
        /// <param name="credentialId">The ID of the SD-JWT credential record to retrieve.</param>
        /// <returns>
        ///     A task representing the asynchronous operation. The task result contains the <see cref="SdJwtRecord" />
        ///     associated with the given ID.
        /// </returns>
        Task<SdJwtRecord> GetAsync(IAgentContext context, string credentialId);

        /// <summary>
        ///     Lists SD-JWT records based on specified criteria.
        /// </summary>
        /// <param name="context">The agent context.</param>
        /// <param name="query">The search query to filter SD-JWT records. Default is null, meaning no filter.</param>
        /// <param name="count">The maximum number of records to retrieve. Default is 100.</param>
        /// <param name="skip">The number of records to skip. Default is 0.</param>
        /// <returns>
        ///     A task representing the asynchronous operation. The task result contains a list of <see cref="SdJwtRecord" />
        ///     that match the criteria.
        /// </returns>
        Task<List<SdJwtRecord>> ListAsync(IAgentContext context, ISearchQuery? query = null, int count = 100,
            int skip = 0);

        /// <summary>
        ///     Stores a new SD-JWT record.
        /// </summary>
        /// <param name="context">The agent context.</param>
        /// <param name="combinedIssuance">The combined issuance.</param>
        /// <param name="keyId">The key id.</param>
        /// <param name="issuerMetadata">The issuer metadata.</param>
        /// <param name="displayMetadata"></param>
        /// <param name="claimMetadata"></param>
        /// <param name="issuerName"></param>
        /// <returns>A task representing the asynchronous operation. The task result contains the ID of the stored JWT record.</returns>
        Task<string> StoreAsync(
            IAgentContext context, 
            string combinedIssuance,
            string keyId, 
            IssuerMetadata issuerMetadata,
            List<CredentialDisplayMetadata> displayMetadata,
            Dictionary<string, ClaimMetadata> claimMetadata,
            Dictionary<string, string> issuerName);
    }
}
