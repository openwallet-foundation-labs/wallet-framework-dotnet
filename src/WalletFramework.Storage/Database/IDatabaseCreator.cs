using LanguageExt;

namespace WalletFramework.Storage.Database;

/// <summary>
/// Initializes the storage schema once during application startup before repositories are used.
/// </summary>
public interface IDatabaseCreator
{
    Task<Unit> EnsureDatabaseCreated(CancellationToken cancellationToken = default);
}
