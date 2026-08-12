using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.Credentials.CredentialSet.Models;
using WalletFramework.Storage.Repositories;

namespace WalletFramework.Credentials.CredentialSet.Persistence;

public class CredentialDataSetRepository(IRepository<CredentialDataSetRecord> repository)
    : ICredentialDataSetStore
{
    public async Task<Unit> AddMany(IEnumerable<CredentialDataSet> credentialDataSets)
    {
        var records = credentialDataSets.Select(dataSet => new CredentialDataSetRecord(dataSet));
        await repository.AddMany(records);
        return Unit.Default;
    }

    public async Task<Unit> Delete(CredentialSetId id)
    {
        var guid = Guid.Parse(id.AsString());
        await repository.RemoveById(guid);
        return Unit.Default;
    }

    public async Task<Option<CredentialDataSet>> Get(CredentialSetId id)
    {
        var guid = Guid.Parse(id.AsString());
        var record = await repository.GetById(guid);
        return record.Map(item => item.ToDomainModel());
    }

    public async Task<IReadOnlyList<CredentialDataSet>> List()
    {
        var records = await repository.ListAll();
        return records.Match<IReadOnlyList<CredentialDataSet>>(
            dataSetRecords => dataSetRecords.Select(record => record.ToDomainModel()).ToList(),
            () => []);
    }

    public async Task<Unit> Save(CredentialDataSet credentialDataSet)
    {
        return await (await Get(credentialDataSet.CredentialSetId)).Match(
            async _ => await UpdateStoredRecord(credentialDataSet),
            async () =>
            {
                var record = new CredentialDataSetRecord(credentialDataSet);
                return await repository.Add(record);
            });
    }

    private async Task<Unit> UpdateStoredRecord(CredentialDataSet credentialDataSet)
    {
        var record = new CredentialDataSetRecord(credentialDataSet);
        await repository.Update(record);
        return Unit.Default;
    }
}
