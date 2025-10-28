using Microsoft.Extensions.DependencyInjection;
using WalletFramework.Core.Credentials;
using WalletFramework.MdocVc;
using WalletFramework.MdocVc.Persistence;
using WalletFramework.Storage;
using WalletFramework.Storage.Database;
using WalletFramework.Storage.Database.DependencyInjection;
using WalletFramework.Storage.Tests.TestModels;
using WalletFramework.Storage.Unencrypted.DependencyInjection;
using WalletFramework.SdJwtVc;
using WalletFramework.SdJwtVc.Persistence;
using WalletFramework.Oid4Vc.CredentialSet.Models;
using WalletFramework.Oid4Vc.CredentialSet.Persistence;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Persistence;
using WalletFramework.Oid4Vc.Oid4Vp.Persistence;
using WalletFramework.Oid4Vc.Oid4Vp;

namespace WalletFramework.Storage.Tests;

public static class TestDbSetup
{
    /// <summary>
    ///     Cleans up test database resources
    /// </summary>
    /// <param name="serviceProvider">The service provider to dispose</param>
    /// <param name="dbPath">The database file path to delete</param>
    public static void Cleanup(ServiceProvider? serviceProvider, string dbPath)
    {
        serviceProvider?.Dispose();
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
    }

    /// <summary>
    ///     Creates a service provider with standard test database configuration
    /// </summary>
    /// <param name="dbPath">Optional custom database path. If null, generates a temporary path.</param>
    /// <returns>A tuple containing the service provider and database path</returns>
    public static (ServiceProvider ServiceProvider, string DbPath) CreateServiceProvider(string? dbPath = null)
    {
        Batteries_V2.Init();

        dbPath ??= Path.Combine(Path.GetTempPath(), $"wf_storage_test_{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={dbPath}";

        var services = new ServiceCollection();

        services.AddUnencryptedSqliteProvider();
        services.ConfigureStorage(connectionString,
            builder =>
            {
                builder.AddRecord<TestRecord, TestRecordConfiguration>();
                builder.AddRecord<MdocCredentialRecord, MdocCredentialRecordConfiguration>();
                builder.AddRecord<SdJwtCredentialRecord, SdJwtCredentialRecordConfiguration>();
                builder.AddRecord<CredentialDataSetRecord, CredentialDataSetRecordConfiguration>();
                builder.AddRecord<CompletedPresentationRecord, CompletedPresentationRecordConfiguration>();
                builder.AddRecord<AuthFlowSessionRecord, AuthFlowSessionRecordConfiguration>();
            });

        services.AddScoped<IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId>, MdocCredentialRepository>();
        services.AddScoped<IDomainRepository<SdJwtCredential, SdJwtCredentialRecord, CredentialId>, SdJwtCredentialRepository>();
        services.AddScoped<IDomainRepository<CredentialDataSet, CredentialDataSetRecord, CredentialSetId>, CredentialDataSetRepository>();
        services.AddScoped<IDomainRepository<CompletedPresentation, CompletedPresentationRecord, string>, CompletedPresentationRepository>();
        services.AddScoped<IDomainRepository<AuthFlowSession, AuthFlowSessionRecord, AuthFlowSessionState>, AuthFlowSessionRepository>();

        var serviceProvider = services.BuildServiceProvider();
        return (serviceProvider, dbPath);
    }

    /// <summary>
    ///     Creates a service provider for simple one-off test scenarios
    /// </summary>
    /// <returns>A tuple containing the service provider and database path</returns>
    public static async Task<(ServiceProvider ServiceProvider, string DbPath)> CreateServiceProviderForSimpleTest()
    {
        var (serviceProvider, dbPath) = CreateServiceProvider();

        await using var scope = serviceProvider.CreateAsyncScope();
        var databaseCreator = scope.ServiceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        return (serviceProvider, dbPath);
    }
}
