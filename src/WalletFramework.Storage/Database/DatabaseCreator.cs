using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WalletFramework.Storage.Database.Exceptions;

namespace WalletFramework.Storage.Database;

public sealed class DatabaseCreator(IDbContextFactory<WalletDbContext> dbContextFactory) : IDatabaseCreator
{
    public async Task<Unit> CreateDatabase(CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var allMigrations = db.Database.GetMigrations();
        if (allMigrations.Any())
        {
            try
            {
                await db.Database.MigrateAsync(cancellationToken);
                return Unit.Default;
            }
            catch (Exception ex)
            {
                throw new DatabaseMigrationException($"Failed to apply migrations: {ex.Message}", ex);
            }
        }
        else
        {
            try
            {
                await db.Database.EnsureCreatedAsync(cancellationToken);
                return Unit.Default;
            }
            catch (Exception ex)
            {
                throw new DatabaseCreationException($"Failed to create database: {ex.Message}", ex);
            }
        }
    }
}
