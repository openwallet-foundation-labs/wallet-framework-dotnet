// using Hyperledger.Aries.Agents;
// using Hyperledger.Aries.Storage;
// using WalletFramework.Oid4Vc.Oid4Vp.Models;
//
// namespace WalletFramework.Oid4Vc.Oid4Vp.Services;
//
// /// <summary>
// ///    This Service offers methods to interact the OpenId4Vp protocol
// /// </summary>
// public interface IOid4VpRecordService
// {
//     /// <summary>
//     ///     Retrieves a specific OidPresentation record by its ID.
//     /// </summary>
//     /// <param name="context">The agent context.</param>
//     /// <param name="presentationId">The ID of the OidPresentation record to retrieve.</param>
//     /// <returns>
//     ///     A task representing the asynchronous operation. The task result contains the <see cref="OidPresentationRecord" />
//     ///     associated with the given ID.
//     /// </returns>
//     Task<OidPresentationRecord> GetAsync(IAgentContext context, string presentationId);
//
//     /// <summary>
//     ///     Lists OidPresentation records based on specified criteria.
//     /// </summary>
//     /// <param name="context">The agent context.</param>
//     /// <param name="query">The search query to filter OidPresentation records. Default is null, meaning no filter.</param>
//     /// <param name="count">The maximum number of records to retrieve. Default is 100.</param>
//     /// <param name="skip">The number of records to skip. Default is 0.</param>
//     /// <returns>
//     ///     A task representing the asynchronous operation. The task result contains a list of <see cref="OidPresentationRecord" />
//     ///     that match the criteria.
//     /// </returns>
//     Task<List<OidPresentationRecord>> ListAsync(IAgentContext context, ISearchQuery? query = null, int count = 100,
//         int skip = 0);
//
//     /// <summary>
//     ///     Stores a new OidPresentation record.
//     /// </summary>
//     /// <param name="context">The agent context.</param>
//     /// <param name="oidPresentationRecord">The presentation record.</param>
//     /// <returns>A task representing the asynchronous operation. The task result contains the ID of the stored OidPresentation record.</returns>
//     Task<string> StoreAsync(IAgentContext context, OidPresentationRecord oidPresentationRecord);
//
//     /// <summary>
//     ///     Deletes a specific OidPresentation record by its ID.
//     /// </summary>
//     /// <param name="context">The agent context.</param>
//     /// <param name="recordId">The ID of the OidPresentation record to delete.</param>
//     /// <returns>
//     ///     A task representing the asynchronous operation. The task result indicates whether the deletion was successful.
//     /// </returns>
//     Task<bool> DeleteAsync(IAgentContext context, string recordId);
// }
