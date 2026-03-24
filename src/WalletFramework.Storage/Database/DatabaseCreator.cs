using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WalletFramework.Storage.Database.Exceptions;

namespace WalletFramework.Storage.Database;

public sealed class DatabaseCreator(IDbContextFactory<WalletDbContext> dbContextFactory) : IDatabaseCreator, IDisposable
{
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _hasInitialized;

    public async Task<Unit> EnsureDatabaseCreated(CancellationToken cancellationToken = default)
    {
        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_hasInitialized)
            {
                return Unit.Default;
            }

            await InitializeDatabase(cancellationToken);
            _hasInitialized = true;
            return Unit.Default;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public void Dispose() => _initializationLock.Dispose();

    private async Task<Unit> InitializeDatabase(CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var hasMigrations = db.Database.GetMigrations().Any();

        if (hasMigrations)
        {
            return await MigrateDatabase(db, cancellationToken);
        }

        return await CreateDatabase(db, cancellationToken);
    }

    private static async Task<Unit> MigrateDatabase(WalletDbContext db, CancellationToken cancellationToken)
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

    private static async Task<Unit> CreateDatabase(WalletDbContext db, CancellationToken cancellationToken)
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
