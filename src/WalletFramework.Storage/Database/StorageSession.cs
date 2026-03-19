using LanguageExt;

namespace WalletFramework.Storage.Database;

public sealed class StorageSession(WalletDbContext context) : IStorageSession
{
    public async Task<Unit> Commit()
    {
        await context.SaveChangesAsync();
        return Unit.Default;
    }
}
