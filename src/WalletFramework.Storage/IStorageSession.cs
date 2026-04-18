using LanguageExt;

namespace WalletFramework.Storage;

public interface IStorageSession
{
    Task<Unit> Commit();
}
