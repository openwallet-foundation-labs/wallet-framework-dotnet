using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Storage;
using WalletFramework.Storage.Repositories;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Persistence;

public class AuthFlowSessionRepository(IRepository<AuthFlowSessionRecord> repository)
    : IDomainRepository<AuthFlowSession, AuthFlowSessionRecord, AuthFlowSessionState>
{
    public async Task<Unit> Add(AuthFlowSession domain)
    {
        var sessionState = domain.AuthFlowSessionState.AsString();

        var existingOpt = await repository.Find(r => r.SessionState == sessionState);
        await existingOpt.Match(
            Some: async list =>
            {
                var existing = list.First();
                var updated = existing with { Serialized = domain.EncodeToJson().ToString() };
                await repository.Update(updated);
            },
            None: async () =>
            {
                await repository.Add(new AuthFlowSessionRecord(domain));
            });

        return Unit.Default;
    }

    public async Task<Unit> AddMany(IEnumerable<AuthFlowSession> domains)
    {
        var records = domains.Select(d => new AuthFlowSessionRecord(d));
        await repository.AddMany(records);
        return Unit.Default;
    }

    public async Task<Unit> Delete(AuthFlowSessionState id)
    {
        var idStr = id.AsString();
        var recs = await repository.Find(r => r.SessionState == idStr);
        await recs.Match(
            Some: async list =>
            {
                foreach (var r in list)
                    await repository.Remove(r);
            },
            None: () => Task.CompletedTask);
        return Unit.Default;
    }

    public async Task<Unit> Delete(AuthFlowSession domain)
    {
        var idStr = domain.AuthFlowSessionState.AsString();
        var recs = await repository.Find(r => r.SessionState == idStr);
        await recs.Match(
            Some: async list =>
            {
                foreach (var r in list)
                    await repository.Remove(r);
            },
            None: () => Task.CompletedTask);
        return Unit.Default;
    }

    public async Task<Option<List<AuthFlowSession>>> Find(ISearchConfig<AuthFlowSessionRecord> config)
    {
        var records = await repository.Find(config.ToPredicate());
        return from recs in records
               let domains = recs.Select(r => r.ToDomainModel())
               select domains.ToList();
    }

    public async Task<Option<AuthFlowSession>> GetById(AuthFlowSessionState id)
    {
        var idStr = id.AsString();
        var records = await repository.Find(r => r.SessionState == idStr);
        return from recs in records
               let record = recs.FirstOrDefault()
               select record?.ToDomainModel();
    }

    public async Task<Option<List<AuthFlowSession>>> ListAll()
    {
        var records = await repository.ListAll();
        return from recs in records
               let domains = recs.Select(r => r.ToDomainModel())
               select domains.ToList();
    }

    public async Task<Unit> Update(AuthFlowSession domain)
    {
        var idStr = domain.AuthFlowSessionState.AsString();
        var existingOpt = await repository.Find(r => r.SessionState == idStr);

        await existingOpt.Match(
            Some: async list =>
            {
                var existing = list.First();
                var updated = existing with { Serialized = domain.EncodeToJson().ToString() };
                await repository.Update(updated);
            },
            None: async () => { await repository.Add(new AuthFlowSessionRecord(domain)); });

        return Unit.Default;
    }
}
