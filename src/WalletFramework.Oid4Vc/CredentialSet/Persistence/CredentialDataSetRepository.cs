using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.Oid4Vc.CredentialSet.Models;
using WalletFramework.Storage;
using WalletFramework.Storage.Repositories;

namespace WalletFramework.Oid4Vc.CredentialSet.Persistence;

public class CredentialDataSetRepository(IRepository<CredentialDataSetRecord> repository)
    : IDomainRepository<CredentialDataSet, CredentialDataSetRecord, CredentialSetId>
{
    public virtual async Task<Unit> Add(CredentialDataSet domain)
    {
        var record = new CredentialDataSetRecord(domain);
        await repository.Add(record);
        return Unit.Default;
    }

    public virtual async Task<Unit> AddMany(IEnumerable<CredentialDataSet> domains)
    {
        var records = domains.Select(domain => new CredentialDataSetRecord(domain));
        await repository.AddMany(records);
        return Unit.Default;
    }

    public virtual async Task<Unit> Delete(CredentialSetId id)
    {
        var guid = Guid.Parse(id.AsString());
        await repository.RemoveById(guid);
        return Unit.Default;
    }

    public virtual async Task<Option<List<CredentialDataSet>>> Find(ISearchConfig<CredentialDataSetRecord> config)
    {
        var records = await repository.Find(config.ToPredicate());
        return from rs in records select rs.Select(r => r.ToDomain()).ToList();
    }

    public virtual async Task<Option<CredentialDataSet>> GetById(CredentialSetId id)
    {
        var guid = Guid.Parse(id.AsString());
        var record = await repository.GetById(guid);
        return record.Map(r => r.ToDomain());
    }

    public virtual async Task<Option<List<CredentialDataSet>>> ListAll()
    {
        var records = await repository.ListAll();
        return from rs in records select rs.Select(r => r.ToDomain()).ToList();
    }

    public virtual async Task<Unit> Update(CredentialDataSet domain)
    {
        var record = new CredentialDataSetRecord(domain);
        await repository.Update(record);
        return Unit.Default;
    }

    public virtual async Task<Unit> Delete(CredentialDataSet domain)
    {
        var record = new CredentialDataSetRecord(domain);
        await repository.Remove(record);
        return Unit.Default;
    }
}
