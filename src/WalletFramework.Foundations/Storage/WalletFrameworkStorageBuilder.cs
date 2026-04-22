using Microsoft.Extensions.DependencyInjection;
using WalletFramework.Storage.Providers;
using WalletFramework.Storage.Records;

namespace WalletFramework.Storage;

internal sealed class WalletFrameworkStorageBuilder(
    WalletFrameworkStorageOptions options) : IWalletFrameworkStorageBuilder
{
    public IWalletFrameworkStorageBuilder AddRecord<TRecord, TConfiguration>()
        where TRecord : RecordBase
        where TConfiguration : class, IRecordConfiguration<TRecord>
    {
        options.AddRecordRegistration(recordsBuilder => recordsBuilder.AddRecord<TRecord, TConfiguration>());

        return this;
    }

    public IWalletFrameworkStorageBuilder AddRecord<TRecord>(
        IRecordConfiguration<TRecord> configuration)
        where TRecord : RecordBase
    {
        options.AddRecordRegistration(recordsBuilder => recordsBuilder.AddRecord(configuration));

        return this;
    }

    public IWalletFrameworkStorageBuilder AutoInitialize()
    {
        options.EnableAutoInitialize();
        return this;
    }

    public IWalletFrameworkStorageBuilder UseConnectionString(string connectionString)
    {
        options.SetConnectionString(connectionString);
        return this;
    }

    public IWalletFrameworkStorageBuilder UseSqliteProvider<TProvider>()
        where TProvider : class, ISqliteProvider
    {
        options.SetSqliteProviderRegistration(services => services.AddSingleton<ISqliteProvider, TProvider>());

        return this;
    }

    public IWalletFrameworkStorageBuilder UseSqliteProvider(
        Func<IServiceProvider, ISqliteProvider> providerFactory)
    {
        options.SetSqliteProviderRegistration(services => services.AddSingleton<ISqliteProvider>(providerFactory));

        return this;
    }
}
