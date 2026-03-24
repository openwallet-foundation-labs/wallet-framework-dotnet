using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.Storage.Repositories;

namespace WalletFramework.SdJwtVc.Persistence;

public class SdJwtCredentialStore(IRepository<SdJwtCredentialRecord> repository)
    : ISdJwtCredentialStore
{
    public async Task<Unit> Delete(CredentialId id)
    {
        var guid = Guid.Parse(id.AsString());
        await repository.RemoveById(guid);
        return Unit.Default;
    }

    public async Task<Option<SdJwtCredential>> Get(CredentialId id)
    {
        var guid = Guid.Parse(id.AsString());
        var record = await repository.GetById(guid);
        return record.Map(item => item.ToDomainModel());
    }

    public async Task<IReadOnlyList<SdJwtCredential>> List()
    {
        var records = await repository.ListAll();
        return records.Match<IReadOnlyList<SdJwtCredential>>(
            credentialRecords => credentialRecords.Select(record => record.ToDomainModel()).ToList(),
            () => []);
    }

    public async Task<Unit> Save(SdJwtCredential credential)
    {
        return await (await Get(credential.CredentialId)).Match(
            async _ => await UpdateStoredRecord(credential),
            async () =>
            {
                var record = new SdJwtCredentialRecord(credential);
                return await repository.Add(record);
            });
    }

    private async Task<Unit> UpdateStoredRecord(SdJwtCredential credential)
    {
        var record = new SdJwtCredentialRecord(credential);
        await repository.Update(record);
        return Unit.Default;
    }
}
