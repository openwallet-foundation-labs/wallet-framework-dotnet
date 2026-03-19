using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Storage.Repositories;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Persistence;

public class AuthFlowSessionRepository(IRepository<AuthFlowSessionRecord> repository)
    : IAuthFlowSessionStore
{
    public async Task<Unit> Save(AuthFlowSession session)
    {
        var sessionState = session.AuthFlowSessionState.AsString();

        var existingOpt = await repository.Find(r => r.SessionState == sessionState);
        await existingOpt.Match(
            Some: async list =>
            {
                var existing = list.First();
                var updated = existing with { Serialized = session.EncodeToJson().ToString() };
                await repository.Update(updated);
            },
            None: async () =>
            {
                await repository.Add(new AuthFlowSessionRecord(session));
            });

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

    public async Task<Option<AuthFlowSession>> Get(AuthFlowSessionState state)
    {
        var stateValue = state.AsString();
        var records = await repository.Find(r => r.SessionState == stateValue);
        return from recs in records
               let record = recs.FirstOrDefault()
               select record?.ToDomainModel();
    }

    public async Task<IReadOnlyList<AuthFlowSession>> List()
    {
        var records = await repository.ListAll();
        return records.Match<IReadOnlyList<AuthFlowSession>>(
            sessionRecords => sessionRecords.Select(record => record.ToDomainModel()).ToList(),
            () => []);
    }
}
