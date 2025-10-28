using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.CredentialSet.Models;
using WalletFramework.Oid4Vc.CredentialSet.Persistence;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.Storage.Database;

namespace WalletFramework.Storage.Tests;

public class CredentialDataSetRecordCrudTests : IDisposable
{
    public CredentialDataSetRecordCrudTests() => (_serviceProvider, _dbPath) = TestDbSetup.CreateServiceProvider();

    private readonly ServiceProvider _serviceProvider;

    private readonly string _dbPath;

    [Fact]
    public async Task Can_Store_And_Retrieve_CredentialSetRecord2()
    {
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider
            .GetRequiredService<IDomainRepository<CredentialDataSet, CredentialDataSetRecord, CredentialSetId>>();

        var setId = CredentialSetId.CreateCredentialSetId();
        var domain = new CredentialDataSet(
            setId,
            Vct.ValidVct("example.vct").ToOption(),
            Option<MdocLib.DocType>.None,
            new Dictionary<string, string> { { "key", "value" } },
            CredentialState.Active,
            Option<Core.StatusList.StatusListEntry>.None,
            Option<DateTime>.None,
            Option<DateTime>.None,
            Option<DateTime>.None,
            Option<DateTime>.None,
            Option<DateTime>.None,
            "https://issuer.example.com");

        await repository.Add(domain);

        var fetched = await repository.GetById(setId);

        fetched.Match(
            found =>
            {
                found.CredentialSetId.Should().Be(setId);
                found.SdJwtCredentialType.IsSome.Should().BeTrue();
                found.CredentialAttributes["key"].Should().Be("value");
            },
            () => throw new InvalidOperationException("Record should exist")
        );
    }

    [Fact]
    public async Task Can_Delete_CredentialDataSetRecord_By_Domain()
    {
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider
            .GetRequiredService<IDomainRepository<CredentialDataSet, CredentialDataSetRecord, CredentialSetId>>();

        var setId = CredentialSetId.CreateCredentialSetId();
        var domain = new CredentialDataSet(
            setId,
            Vct.ValidVct("example.vct").ToOption(),
            Option<MdocLib.DocType>.None,
            new Dictionary<string, string> { { "key", "value" } },
            CredentialState.Active,
            Option<Core.StatusList.StatusListEntry>.None,
            Option<DateTime>.None,
            Option<DateTime>.None,
            Option<DateTime>.None,
            Option<DateTime>.None,
            Option<DateTime>.None,
            "https://issuer.example.com");

        await repository.Add(domain);

        await repository.Delete(domain);

        var fetched = await repository.GetById(setId);
        fetched.IsNone.Should().BeTrue();
    }

    public void Dispose()
    {
        TestDbSetup.Cleanup(_serviceProvider, _dbPath);
        GC.SuppressFinalize(this);
    }
}
