using LanguageExt;
using WalletFramework.Storage.Repositories;

namespace WalletFramework.Oid4Vc.Oid4Vp.Persistence;

public class CompletedPresentationRepository(IRepository<CompletedPresentationRecord> repository)
    : ICompletedPresentationStore
{
    public async Task<Unit> Add(CompletedPresentation presentation)
    {
        await repository.Add(new CompletedPresentationRecord(presentation));
        return Unit.Default;
    }

    public async Task<Option<CompletedPresentation>> Get(string presentationId)
    {
        var records = await repository.Find(record => record.PresentationId == presentationId);
        return from matchedRecords in records
               let record = matchedRecords.FirstOrDefault()
               select record?.ToDomainModel();
    }

    public async Task<IReadOnlyList<CompletedPresentation>> List()
    {
        var records = await repository.ListAll();
        return MapRecords(records);
    }

    public async Task<IReadOnlyList<CompletedPresentation>> ListByClientId(string clientId)
    {
        var records = await repository.Find(record => record.ClientId == clientId);
        return MapRecords(records);
    }

    public async Task<Unit> Save(CompletedPresentation presentation)
    {
        var existingOpt = await repository.Find(record => record.ClientId == presentation.ClientId);

        await existingOpt.Match(
            Some: async list =>
            {
                var existing = list[0];
                var updated = existing with { Serialized = presentation.Serialize() };
                await repository.Update(updated);
            },
            None: async () =>
            {
                await repository.Add(new CompletedPresentationRecord(presentation));
            });

        return Unit.Default;
    }

    public async Task<Unit> Delete(string presentationId)
    {
        var records = await repository.Find(record => record.PresentationId == presentationId);
        await records.Match(
            Some: async matchedRecords =>
            {
                foreach (var record in matchedRecords)
                {
                    await repository.Remove(record);
                }
            },
            None: () => Task.CompletedTask);

        return Unit.Default;
    }

    private static IReadOnlyList<CompletedPresentation> MapRecords(
        Option<IReadOnlyList<CompletedPresentationRecord>> records)
    {
        return records.Match<IReadOnlyList<CompletedPresentation>>(
            presentationRecords => presentationRecords.Select(record => record.ToDomainModel()).ToList(),
            () => []);
    }
}
