using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.Storage;
using WalletFramework.Storage.Repositories;

namespace WalletFramework.SdJwtVc.Persistence;

public class SdJwtCredentialRepository(IRepository<SdJwtCredentialRecord> repository)
    : IDomainRepository<SdJwtCredential, SdJwtCredentialRecord, CredentialId>
{
    public async Task<Unit> Add(SdJwtCredential domain)
    {
        var record = new SdJwtCredentialRecord(domain);
        await repository.Add(record);
        return Unit.Default;
    }

    public async Task<Unit> AddMany(IEnumerable<SdJwtCredential> domains)
    {
        var records = domains.Select(domain => new SdJwtCredentialRecord(domain));
        await repository.AddMany(records);
        return Unit.Default;
    }

    public async Task<Option<SdJwtCredential>> GetById(CredentialId id)
    {
        var guid = Guid.Parse(id.AsString());
        var record = await repository.GetById(guid);
        return record.Map(r => r.ToDomain());
    }

    public async Task<Option<List<SdJwtCredential>>> Find(ISearchConfig<SdJwtCredentialRecord> config)
    {
        var records = await repository.Find(config.ToPredicate());
        return
            from credentialRecords in records
            let credentials = credentialRecords.Select(r => r.ToDomain())
            select credentials.ToList();
    }

    public async Task<Option<List<SdJwtCredential>>> ListAll()
    {
        var records = await repository.ListAll();
        return
            from credentialRecords in records
            let credentials = credentialRecords.Select(r => r.ToDomain())
            select credentials.ToList();
    }

    public async Task<Unit> Update(SdJwtCredential domain)
    {
        var record = new SdJwtCredentialRecord(domain);
        await repository.Update(record);
        return Unit.Default;
    }

    public async Task<Unit> Delete(CredentialId id)
    {
        var guid = Guid.Parse(id.AsString());
        await repository.RemoveById(guid);
        return Unit.Default;
    }

    public async Task<Unit> Delete(SdJwtCredential domain)
    {
        var record = new SdJwtCredentialRecord(domain);
        await repository.Remove(record);
        return Unit.Default;
    }
}
