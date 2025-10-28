using LanguageExt;

namespace WalletFramework.Storage.Database;

public interface IDatabaseCreator
{
    Task<Unit> CreateDatabase(CancellationToken cancellationToken = default);
}
