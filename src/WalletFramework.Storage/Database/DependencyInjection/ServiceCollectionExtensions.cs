using Microsoft.Extensions.DependencyInjection;
using WalletFramework.Storage.Providers;
using WalletFramework.Storage.Records;
using WalletFramework.Storage.Repositories;

namespace WalletFramework.Storage.Database.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Creates a storage builder resolving the SQLite provider from DI so callers don't need to construct it.
    /// Storage must be initialized once after the service provider has been built and before repositories are used.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQLite connection string.</param>
    /// <param name="configure">Action to configure the storage builder with records.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ConfigureStorage(
        this IServiceCollection services,
        string connectionString,
        Action<IRecordsBuilder> configure)
    {
        var builder = new RecordsBuilder(services);

        builder.AddRecord(new RecordBaseConfiguration());

        configure(builder);

        services.AddDbContextFactory<WalletDbContext>((sp, options) =>
        {
            var sqliteProvider = sp.GetRequiredService<ISqliteProvider>();
            sqliteProvider.Initialize();
            sqliteProvider.Configure(options, connectionString);
        });

        services.AddSingleton<DatabaseCreator>();
        services.AddSingleton<IDatabaseCreator>(sp => sp.GetRequiredService<DatabaseCreator>());

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IStorageSession, StorageSession>();

        return services;
    }
}
