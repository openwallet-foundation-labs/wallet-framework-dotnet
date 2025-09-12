using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.Storage;
using WalletFramework.Storage.Repositories;

namespace WalletFramework.MdocVc.Persistence;

public class MdocCredentialRepository(IRepository<MdocCredentialRecord> repository)
    : IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId>
{
    public async Task<Unit> Add(MdocCredential domainModel)
    {
        var record = new MdocCredentialRecord(domainModel);
        await repository.Add(record);
        return Unit.Default;
    }

    public async Task<Unit> AddMany(IEnumerable<MdocCredential> domainModels)
    {
        var records = domainModels.Select(d => new MdocCredentialRecord(d));
        await repository.AddMany(records);
        return Unit.Default;
    }

    public virtual async Task<Unit> Delete(CredentialId id)
    {
        var guid = Guid.Parse(id.AsString());
        await repository.RemoveById(guid);
        return Unit.Default;
    }

    public virtual async Task<Unit> Delete(MdocCredential domainModel)
    {
        var record = new MdocCredentialRecord(domainModel);
        await repository.Remove(record);
        return Unit.Default;
    }

    public async Task<Option<List<MdocCredential>>> Find(ISearchConfig<MdocCredentialRecord> config)
    {
        var records = await repository.Find(config.ToPredicate());
        return
            from credentialRecords in records
            let credentials = credentialRecords.Select(r => r.ToDomainModel())
            select credentials.ToList();
    }

    public async Task<Option<MdocCredential>> GetById(CredentialId id)
    {
        var guid = Guid.Parse(id.AsString());
        var record = await repository.GetById(guid);
        return record.Map(r => r.ToDomainModel());
    }

    public async Task<Option<List<MdocCredential>>> ListAll()
    {
        var records = await repository.ListAll();
        return
            from credentialRecords in records
            let credentials = credentialRecords.Select(r => r.ToDomainModel())
            select credentials.ToList();
    }

    public virtual async Task<Unit> Update(MdocCredential domainModel)
    {
        var record = new MdocCredentialRecord(domainModel);
        await repository.Update(record);
        return Unit.Default;
    }
}
