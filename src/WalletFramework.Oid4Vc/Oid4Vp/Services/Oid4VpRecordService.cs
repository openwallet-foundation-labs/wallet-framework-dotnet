using Hyperledger.Aries;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

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
        List<PresentedCredentialSet> presentedCredentialSets)
    {
        var record = new OidPresentationRecord(
            presentedCredentialSets,
            clientId,
            Guid.NewGuid().ToString(),
            clientMetadata,
            name
        );
            
        await RecordService.AddAsync(context.Wallet, record);
            
        return record.Id;
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(IAgentContext context, string recordId)
    {
        return RecordService.DeleteAsync<OidPresentationRecord>(context.Wallet, recordId);
    }
}
