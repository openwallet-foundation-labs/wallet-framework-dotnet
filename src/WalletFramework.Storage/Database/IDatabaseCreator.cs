using LanguageExt;

namespace WalletFramework.Storage.Database;

public interface IDatabaseCreator
{
    Task<Unit> EnsureDatabaseCreated(CancellationToken cancellationToken = default);
}
