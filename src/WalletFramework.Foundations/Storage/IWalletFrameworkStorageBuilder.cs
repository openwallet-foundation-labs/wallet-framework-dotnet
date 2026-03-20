using WalletFramework.Storage.Providers;
using WalletFramework.Storage.Records;

namespace WalletFramework.Storage;

public interface IWalletFrameworkStorageBuilder
{
    IWalletFrameworkStorageBuilder AddRecord<TRecord, TConfiguration>()
        where TRecord : RecordBase
        where TConfiguration : class, IRecordConfiguration<TRecord>;

    IWalletFrameworkStorageBuilder AddRecord<TRecord>(IRecordConfiguration<TRecord> configuration)
        where TRecord : RecordBase;

    IWalletFrameworkStorageBuilder AutoInitialize();
    
    IWalletFrameworkStorageBuilder UseConnectionString(string connectionString);

    IWalletFrameworkStorageBuilder UseSqliteProvider<TProvider>()
        where TProvider : class, ISqliteProvider;

    IWalletFrameworkStorageBuilder UseSqliteProvider(Func<IServiceProvider, ISqliteProvider> providerFactory);
}
