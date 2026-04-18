using System.Linq.Expressions;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WalletFramework.Storage.Database;
using WalletFramework.Storage.Records;
using static LanguageExt.Prelude;

namespace WalletFramework.Storage.Repositories;

public sealed class Repository<TRecord>(WalletDbContext context) : IRepository<TRecord>
    where TRecord : RecordBase
{
    public async Task<Unit> Add(TRecord record)
    {
        await context.Set<TRecord>().AddAsync(record);
        return Unit.Default;
    }

    public async Task<Unit> AddMany(IEnumerable<TRecord> records)
    {
        await context.Set<TRecord>().AddRangeAsync(records);
        return Unit.Default;
    }

    public async Task<Option<IReadOnlyList<TRecord>>> Find(Expression<Func<TRecord, bool>> predicate)
    {
        var results = await context
            .Set<TRecord>()
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync();
        return results.Count == 0 ? Option<IReadOnlyList<TRecord>>.None : results;
    }

    public async Task<Option<TRecord>> GetById(Guid id)
    {
        var entity = await context
            .Set<TRecord>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.RecordId == id);
        return Optional(entity);
    }

    public async Task<Option<IReadOnlyList<TRecord>>> ListAll()
    {
        var results = await context
            .Set<TRecord>()
            .AsNoTracking()
            .ToListAsync();
        return results.Count == 0 ? Option<IReadOnlyList<TRecord>>.None : results;
    }

    public Task<Unit> Remove(TRecord record)
    {
        var trackedRecord = context.Set<TRecord>().Local.FirstOrDefault(entity => entity.RecordId == record.RecordId);
        if (trackedRecord is not null)
        {
            context.Set<TRecord>().Remove(trackedRecord);
            return Task.FromResult(Unit.Default);
        }

        context.Set<TRecord>().Remove(record);
        return Task.FromResult(Unit.Default);
    }

    public async Task<Unit> RemoveById(Guid id)
    {
        var trackedEntity = context.Set<TRecord>().Local.FirstOrDefault(entity => entity.RecordId == id);
        if (trackedEntity is not null)
        {
            context.Set<TRecord>().Remove(trackedEntity);
            return Unit.Default;
        }

        var entity = await context.Set<TRecord>().FirstOrDefaultAsync(e => e.RecordId == id);
        if (entity is not null)
        {
            context.Set<TRecord>().Remove(entity);
        }

        return Unit.Default;
    }

    public async Task<Unit> Update(TRecord record)
    {
        var trackedRecord = context.Set<TRecord>().Local.FirstOrDefault(entity => entity.RecordId == record.RecordId);
        var oldRecord = trackedRecord
                        ?? await context.Set<TRecord>().AsNoTracking().SingleAsync(e => e.RecordId == record.RecordId);

        record = record with
        {
            CreatedAt = oldRecord.CreatedAt,
            UpdatedAt = DateTimeOffset.Now
        };

        if (trackedRecord is not null)
        {
            var trackedRecordEntry = context.Entry(trackedRecord);
            var trackedRecordState = trackedRecordEntry.State;
            trackedRecordEntry.CurrentValues.SetValues(record);
            if (trackedRecordState != EntityState.Added)
            {
                trackedRecordEntry.State = EntityState.Modified;
            }

            return Unit.Default;
        }

        context.Set<TRecord>().Update(record);
        return Unit.Default;
    }
}
