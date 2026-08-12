using Microsoft.Extensions.DependencyInjection;
using WalletFramework.Storage.Records;

namespace WalletFramework.Storage;

internal sealed class WalletFrameworkStorageOptions
{
    internal bool AutoInitializeEnabled { get; private set; }
    
    private Action<IRecordsBuilder>? _recordRegistration;
    private Action<IServiceCollection>? _sqliteProviderRegistration;
    
    private string? _connectionString;

    internal void AddRecordRegistration(Action<IRecordsBuilder> recordRegistration)
        => _recordRegistration += recordRegistration;

    internal void ConfigureRecords(IRecordsBuilder recordsBuilder) =>
        _recordRegistration?.Invoke(recordsBuilder);

    internal void EnableAutoInitialize() => AutoInitializeEnabled = true;

    internal string GetConnectionString()
    {
        if (_connectionString is { } connectionString &&
            !string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        throw new InvalidOperationException(
            "Storage configuration requires a connection string. " +
            "Use UseConnectionString(...) inside UseStorage(...).");
    }

    internal void RegisterSqliteProvider(IServiceCollection services)
    {
        if (_sqliteProviderRegistration is { } sqliteProviderRegistration)
        {
            sqliteProviderRegistration(services);
            return;
        }

        throw new InvalidOperationException(
            "Storage configuration requires an ISqliteProvider registration. " +
            "Use UseSqliteProvider(...) inside UseStorage(...).");
    }

    internal void SetConnectionString(string connectionString)
    {
        var hasConnectionString = string.IsNullOrWhiteSpace(connectionString) is false;

        if (hasConnectionString)
        {
            _connectionString = connectionString;
            return;
        }

        throw new InvalidOperationException(
            "Storage configuration requires a non-empty connection string.");
    }

    internal void SetSqliteProviderRegistration(Action<IServiceCollection> sqliteProviderRegistration)
        => _sqliteProviderRegistration = sqliteProviderRegistration;
}
