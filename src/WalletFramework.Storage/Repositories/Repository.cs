using System.Linq.Expressions;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WalletFramework.Storage.Database;
using WalletFramework.Storage.Records;
using static LanguageExt.Prelude;

namespace WalletFramework.Storage.Repositories;

public sealed class Repository<TRecord>(IDbContextFactory<WalletDbContext> dbContextFactory) : IRepository<TRecord>
    where TRecord : RecordBase
{
    public async Task<Unit> Add(TRecord record)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        await context.Set<TRecord>().AddAsync(record);
        await context.SaveChangesAsync();
        return Unit.Default;
    }

    public async Task<Unit> AddMany(IEnumerable<TRecord> records)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        await context.Set<TRecord>().AddRangeAsync(records);
        await context.SaveChangesAsync();
        return Unit.Default;
    }

    public async Task<Option<IReadOnlyList<TRecord>>> Find(Expression<Func<TRecord, bool>> predicate)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var results = await context
            .Set<TRecord>()
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync();
        return results.Count == 0 ? Option<IReadOnlyList<TRecord>>.None : results;
    }

    public async Task<Option<TRecord>> GetById(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var entity = await context
            .Set<TRecord>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.RecordId == id);
        return Optional(entity);
    }

    public async Task<Option<IReadOnlyList<TRecord>>> ListAll()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var results = await context
            .Set<TRecord>()
            .AsNoTracking()
            .ToListAsync();
        return results.Count == 0 ? Option<IReadOnlyList<TRecord>>.None : results;
    }

    public async Task<Unit> Remove(TRecord record)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        context.Set<TRecord>().Remove(record);
        await context.SaveChangesAsync();
        return Unit.Default;
    }

    public async Task<Unit> RemoveById(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var entity = await context.Set<TRecord>().FirstOrDefaultAsync(e => e.RecordId == id);
        if (entity is null)
        {
            return Unit.Default;
        }

        context.Set<TRecord>().Remove(entity);
        await context.SaveChangesAsync();
        return Unit.Default;
    }

    public async Task<Unit> Update(TRecord record)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        record = record with { UpdatedAt = DateTimeOffset.Now };
        
        context.Set<TRecord>().Update(record);
        await context.SaveChangesAsync();
        return Unit.Default;
    }
}
