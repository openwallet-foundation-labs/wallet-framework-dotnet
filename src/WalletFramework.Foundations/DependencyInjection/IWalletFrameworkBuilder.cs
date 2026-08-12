using WalletFramework.Storage;

namespace WalletFramework.DependencyInjection;

public interface IWalletFrameworkBuilder
{
    IWalletFrameworkBuilder UseStorage(Action<IWalletFrameworkStorageBuilder> configure);
}
