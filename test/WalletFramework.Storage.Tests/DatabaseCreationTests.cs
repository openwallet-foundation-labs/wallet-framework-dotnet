using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WalletFramework.Storage.Database;
using WalletFramework.Storage.Database.DependencyInjection;
using WalletFramework.Storage.Unencrypted.DependencyInjection;
using WalletFramework.Storage.Tests.TestModels;

namespace WalletFramework.Storage.Tests;

public class DatabaseCreationTests
{
    [Fact]
    public async Task Can_Create_Database()
    {
        var (provider, dbPath) = await TestDbSetup.CreateServiceProviderForSimpleTest();

        await using (provider)
        {
            File.Exists(dbPath).Should().BeTrue();
        }

        // Clean up
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
    }
}
