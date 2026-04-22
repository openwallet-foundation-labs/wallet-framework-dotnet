using WalletFramework.Storage;

namespace WalletFramework.DependencyInjection;

internal sealed class WalletFrameworkBuilder : IWalletFrameworkBuilder
{
    internal WalletFrameworkStorageOptions? StorageOptions { get; private set; }

    public IWalletFrameworkBuilder UseStorage(Action<IWalletFrameworkStorageBuilder> configure)
    {
        var storageOptions = StorageOptions ?? new WalletFrameworkStorageOptions();
        var storageBuilder = new WalletFrameworkStorageBuilder(storageOptions);

        configure(storageBuilder);

        StorageOptions = storageOptions;
        return this;
    }
}
