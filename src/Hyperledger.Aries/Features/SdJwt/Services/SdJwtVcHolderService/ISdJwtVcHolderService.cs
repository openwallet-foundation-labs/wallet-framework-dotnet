using System.Collections.Generic;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Issuer;
using Hyperledger.Aries.Features.OpenId4Vc.Vp.Models;
using Hyperledger.Aries.Features.Pex.Models;
using Hyperledger.Aries.Features.SdJwt.Models.Records;
using Hyperledger.Aries.Storage;

namespace Hyperledger.Aries.Features.SdJwt.Services.SdJwtVcHolderService
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
        ///     Finds the credential candidates based on the provided credentials and input descriptors.
        /// </summary>
        /// <param name="credentials">An array of available credentials.</param>
        /// <param name="inputDescriptors">An array of input descriptors to be satisfied.</param>
        /// <returns>An array of credential candidates, each containing a list of credentials that match the input descriptors.</returns>
        Task<CredentialCandidates[]> FindCredentialCandidates(SdJwtRecord[] credentials,
            InputDescriptor[] inputDescriptors);

        /// <summary>
        ///     Creates a SD-JWT in presentation format where the provided claims are disclosed.
        ///     The key binding is optional and can be activated by providing an audience and a nonce.
        /// </summary>
        /// <remarks>
        ///     The SD-JWT is created using the provided SD-JWT credential and the provided claims are disclosed
        /// </remarks>
        /// <param name="disclosureNames">The claims to disclose</param>
        /// <param name="credential">The SD-JWT credential</param>
        /// <param name="audience">The targeted audience</param>
        /// <param name="nonce">The nonce</param>
        /// <returns>The SD-JWT in presentation format</returns>
        Task<string> CreatePresentation(SdJwtRecord credential, string[]? disclosureNames, string? audience = null, string? nonce = null);
        
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
        /// <param name="credentialMetadataId">The credential metadata ID.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the ID of the stored JWT record.</returns>
        Task<string> StoreAsync(
            IAgentContext context,
            string combinedIssuance,
            string keyId,
            OidIssuerMetadata issuerMetadata,
            string credentialMetadataId);
    }
}
