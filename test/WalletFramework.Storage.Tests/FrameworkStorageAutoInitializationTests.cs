using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WalletFramework.DependencyInjection;
using WalletFramework.Storage.Unencrypted;

namespace WalletFramework.Storage.Tests;

public sealed class FrameworkStorageAutoInitializationTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(),
        $"wallet_framework_auto_initialize_{Guid.NewGuid():N}.db");

    [Fact]
    public void AutoInitialize_Creates_Database_During_Framework_Registration()
    {
        var services = new ServiceCollection();

        services.AddWalletFramework(builder =>
        {
            builder.UseStorage(storage =>
            {
                storage.UseConnectionString($"Data Source={_dbPath}");
                storage.UseSqliteProvider<Sqlite3Provider>();
                storage.AutoInitialize();
            });
        });

        File.Exists(_dbPath).Should().BeTrue("auto initialization should create the sqlite database immediately");
    }

    public void Dispose()
    {
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
