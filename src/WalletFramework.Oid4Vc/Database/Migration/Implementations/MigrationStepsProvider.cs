using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using LanguageExt;
using WalletFramework.MdocVc;
using WalletFramework.Oid4Vc.CredentialSet;
using WalletFramework.Oid4Vc.CredentialSet.Models;
using WalletFramework.Oid4Vc.Database.Migration.Abstraction;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.Database.Migration.Implementations;

internal class MigrationStepsProvider(
    IAgentProvider agentProvider,
    ICredentialSetStorage credentialSetStorage,
    IMdocStorage mdocStorage,
    ISdJwtVcHolderService sdJwtVcHolderService) : IMigrationStepsProvider
{
    public IEnumerable<MigrationStep> Get()
    {
        var sdJwtStep = new MigrationStep(
            1,
            2,
            typeof(SdJwtRecord),
            async records =>
            {
                var sdJwtRecords = records.OfType<SdJwtRecord>();
                foreach (var record in sdJwtRecords)
                {
                    if (string.IsNullOrWhiteSpace(record.CredentialSetId))
                    {
                        var credentialSetRecord = new CredentialSetRecord();
                        credentialSetRecord.AddSdJwtData(record);
                        await credentialSetStorage.Add(credentialSetRecord);
                        record.CredentialSetId = credentialSetRecord.CredentialSetId;
                    }
                    
                    record.RecordVersion = 2;
                    var context = await agentProvider.GetContextAsync();
                    await sdJwtVcHolderService.UpdateAsync(context, record);
                }
            },
            async () =>
            {
                var query = SearchQuery.Less(
                    nameof(MdocRecord.RecordVersion),
                    "2");

                var context = await agentProvider.GetContextAsync();
                var records = await sdJwtVcHolderService.ListAsync(context, query);
                return records.Any() 
                    ? records.Cast<RecordBase>().ToList() 
                    : Option<IEnumerable<RecordBase>>.None;
            });

        var mdocStep = new MigrationStep(
            1,
            2,
            typeof(MdocRecord),
            async records =>
            {
                var mdocRecords = records.OfType<MdocRecord>();
                foreach (var record in mdocRecords)
                {
                    if (string.IsNullOrWhiteSpace(record.CredentialSetId))
                    {
                        var credentialSetRecord = new CredentialSetRecord();
                        credentialSetRecord.AddMdocData(record);
                        await credentialSetStorage.Add(credentialSetRecord);
                        record.CredentialSetId = credentialSetRecord.CredentialSetId;
                    }
                    
                    record.RecordVersion = 2;
                    await mdocStorage.Update(record);
                }
            }, 
            async () =>
            {
                var query = SearchQuery.Not(SearchQuery.Greater(nameof(MdocRecord.RecordVersion), "1"));
                var someQuery = Option<ISearchQuery>.Some(query);

                var mdocs = await mdocStorage.List(someQuery);
                return
                    from mdocRecords in mdocs
                    select mdocRecords.Cast<RecordBase>();
            });

        return [sdJwtStep, mdocStep];
    }
}
