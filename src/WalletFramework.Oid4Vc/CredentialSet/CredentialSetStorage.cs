using Hyperledger.Aries;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.Oid4Vc.CredentialSet.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.CredentialSet;

public class CredentialSetStorage(
    IAgentProvider agentProvider,
    ISdJwtVcHolderService sdJwtVcHolderService,
    IMdocStorage mDocStorage,
    IWalletRecordService walletRecordService) : ICredentialSetStorage
{
    public async Task Add(CredentialSetRecord credentialSetRecord)
    {
        var context = await agentProvider.GetContextAsync();
        await walletRecordService.AddAsync(context.Wallet, credentialSetRecord);
    }

    public virtual async Task Delete(CredentialSetId credentialSetId)
    {
        var context = await agentProvider.GetContextAsync();
        var credentialSetRecord =
            await walletRecordService.GetAsync<CredentialSetRecord>(context.Wallet, credentialSetId);
        if (credentialSetRecord == null)
            throw new AriesFrameworkException(ErrorCode.RecordNotFound, "CredentialSet record not found");

        var sdJwtRecords = await sdJwtVcHolderService.ListAsync(context, credentialSetId);
        await sdJwtRecords.Match(
            Some: async records =>
            {
                foreach (var record in records)
                    await sdJwtVcHolderService.DeleteAsync(context, record.Id);
            },
            None: () => Task.CompletedTask);

        var mDocRecords = await mDocStorage.List(credentialSetId);
        await mDocRecords.Match(
            Some: async records =>
            {
                foreach (var record in records)
                    await mDocStorage.Delete(record);
            },
            None: () => Task.CompletedTask);

        credentialSetRecord.State = CredentialState.Deleted;
        credentialSetRecord.DeletedAt = DateTime.UtcNow;
        await walletRecordService.UpdateAsync(context.Wallet, credentialSetRecord);
    }

    public async Task<Option<CredentialSetRecord>> Get(CredentialSetId credentialSetId)
    {
        var context = await agentProvider.GetContextAsync();
        var record = await walletRecordService.GetAsync<CredentialSetRecord>(context.Wallet, credentialSetId);

        return record;
    }

    public async Task<Option<IEnumerable<CredentialSetRecord>>> List(
        Option<ISearchQuery> query,
        int count = 100,
        int skip = 0)
    {
        var context = await agentProvider.GetContextAsync();
        var records = await walletRecordService.SearchAsync<CredentialSetRecord>(
            context.Wallet,
            query.ToNullable(),
            null,
            count,
            skip);

        if (records.Count == 0)
            return Option<IEnumerable<CredentialSetRecord>>.None;

        return records;
    }
    

    public virtual async Task Update(CredentialSetRecord credentialSetRecord)
    {
        var context = await agentProvider.GetContextAsync();
        await walletRecordService.UpdateAsync(context.Wallet, credentialSetRecord);
    }
}
