using LanguageExt;
using Newtonsoft.Json;
using WalletFramework.Storage;
using WalletFramework.Storage.Repositories;

namespace WalletFramework.Oid4Vc.Oid4Vp.Persistence;

public class CompletedPresentationRepository(IRepository<CompletedPresentationRecord> repository)
    : IDomainRepository<CompletedPresentation, CompletedPresentationRecord, string>
{
    public async Task<Unit> Add(CompletedPresentation domain)
    {
        var toStore = domain with { LastTimeUsed = DateTimeOffset.UtcNow };
        await repository.Add(new CompletedPresentationRecord(toStore));
        return Unit.Default;
    }

    public async Task<Unit> AddMany(IEnumerable<CompletedPresentation> domains)
    {
        var now = DateTimeOffset.UtcNow;
        var records = domains.Select(d => new CompletedPresentationRecord(d with { LastTimeUsed = now }));
        await repository.AddMany(records);
        return Unit.Default;
    }

    public async Task<Unit> Delete(string id)
    {
        var recs = await repository.Find(r => r.PresentationId == id);
        await recs.Match(
            Some: async list =>
            {
                foreach (var record in list)
                {
                    await repository.Remove(record);
                }
            },
            None: () => Task.CompletedTask);

        return Unit.Default;
    }

    public async Task<Unit> Delete(CompletedPresentation domain)
    {
        var recs = await repository.Find(r => r.PresentationId == domain.PresentationId);
        await recs.Match(
            Some: async list =>
            {
                foreach (var record in list)
                {
                    await repository.Remove(record);
                }
            },
            None: () => Task.CompletedTask);

        return Unit.Default;
    }

    public async Task<Option<List<CompletedPresentation>>> Find(ISearchConfig<CompletedPresentationRecord> config)
    {
        var records = await repository.Find(config.ToPredicate());
        return from recs in records
               let domains = recs.Select(r => r.ToDomain())
               select domains.ToList();
    }

    public async Task<Option<CompletedPresentation>> GetById(string id)
    {
        var records = await repository.Find(r => r.PresentationId == id);
        return from recs in records
               let record = recs.FirstOrDefault()
               select record?.ToDomain();
    }

    public async Task<Option<List<CompletedPresentation>>> ListAll()
    {
        var records = await repository.ListAll();
        return from recs in records
               let domains = recs.Select(r => r.ToDomain())
               select domains.ToList();
    }

    public async Task<Unit> Update(CompletedPresentation domain)
    {
        var existingOpt = await repository.Find(r => r.ClientId == domain.ClientId);

        await existingOpt.Match(
            Some: async list =>
            {
                var existing = list.First();
                var updated = existing with { Serialized = JsonConvert.SerializeObject(domain) };
                await repository.Update(updated);
            },
            None: async () =>
            {
                var toStore = domain with { LastTimeUsed = DateTimeOffset.UtcNow };
                await repository.Add(new CompletedPresentationRecord(toStore));
            });

        return Unit.Default;
    }
}
