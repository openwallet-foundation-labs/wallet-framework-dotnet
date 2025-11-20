using Microsoft.Extensions.DependencyInjection;
using WalletFramework.Storage.Providers;

namespace WalletFramework.Storage.Unencrypted.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// WARNING: This provider stores data without encryption. Do not use for sensitive data.
    /// </summary>
    public static IServiceCollection AddUnencryptedSqliteProvider(
        this IServiceCollection services)
    {
        services.AddSingleton<ISqliteProvider, Sqlite3Provider>();
        return services;
    }
}
