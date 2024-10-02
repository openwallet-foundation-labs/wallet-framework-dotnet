using Hyperledger.Aries;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.MdocVc;
using WalletFramework.Oid4Vc.CredentialSet.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.CredentialSet;

public class CredentialSetService : ICredentialSetService
{
    private readonly IAgentProvider _agentProvider;
    private readonly ISdJwtVcHolderService _sdJwtVcHolderService;
    private readonly IMdocStorage _mDocStorage;
    private readonly IWalletRecordService _walletRecordService;

    public CredentialSetService(
        IAgentProvider agentProvider,
        ISdJwtVcHolderService sdJwtVcHolderService,
        IMdocStorage mDocStorage,
        IWalletRecordService walletRecordService)
    {
        _agentProvider = agentProvider;
        _sdJwtVcHolderService = sdJwtVcHolderService;
        _mDocStorage = mDocStorage;
        _walletRecordService = walletRecordService;
    }
    
    public async Task<Option<IEnumerable<SdJwtRecord>>> GetAssociatedSdJwtRecords(string credentialSetId)
    {
        var context = await _agentProvider.GetContextAsync();
        
        var sdJwtQuery = SearchQuery.Equal(
            "~" + nameof(SdJwtRecord.CredentialSetId),
            credentialSetId);

        var sdJwtRecords = await _sdJwtVcHolderService.ListAsync(
            context,
            sdJwtQuery);

        return sdJwtRecords.Any() 
            ? sdJwtRecords 
            : Option<IEnumerable<SdJwtRecord>>.None;
    }
    
    public async Task<Option<IEnumerable<MdocRecord>>> GetAssociatedMDocRecords(string credentialSetId)
    {
        var mDocQuery = SearchQuery.Equal(
            "~" + nameof(MdocRecord.CredentialSetId),
            credentialSetId);

        return await _mDocStorage.List(
            Option<ISearchQuery>.Some(mDocQuery));
    }

    public async Task DeleteAsync(string credentialSetId)
    {
        var context = await _agentProvider.GetContextAsync();
        var credentialSetRecord = await _walletRecordService.GetAsync<CredentialSetRecord>(context.Wallet, credentialSetId);
        if (credentialSetRecord == null)
            throw new AriesFrameworkException(ErrorCode.RecordNotFound, "CredentialSet record not found");
        
        var sdJwtRecords = await GetAssociatedSdJwtRecords(credentialSetId);
        await sdJwtRecords.Match(
            Some: async records =>
            {
                foreach (var record in records)
                    await _sdJwtVcHolderService.DeleteAsync(context, record.Id);
            },
            None: () => Task.CompletedTask);
        
        var mDocRecords = await GetAssociatedMDocRecords(credentialSetId);
        await mDocRecords.Match(
            Some: async records =>
            {
                foreach (var record in records)
                    await _mDocStorage.Delete(record);
            },
            None: () => Task.CompletedTask);

        credentialSetRecord.State = CredentialState.DELETED;
        credentialSetRecord.DeletedAt = DateTime.UtcNow;
        await _walletRecordService.UpdateAsync(context.Wallet, credentialSetRecord);
    }

    public async Task AddAsync(CredentialSetRecord credentialSetRecord)
    {
        var context = await _agentProvider.GetContextAsync();
        await _walletRecordService.AddAsync(context.Wallet, credentialSetRecord);
    }
    
    public async Task UpdateAsync(CredentialSetRecord credentialSetRecord)
    {
        var context = await _agentProvider.GetContextAsync();
        await _walletRecordService.UpdateAsync(context.Wallet, credentialSetRecord);
    }
}
