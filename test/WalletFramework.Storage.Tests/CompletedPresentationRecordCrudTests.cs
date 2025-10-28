using System.Linq.Expressions;
using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using WalletFramework.Core.Credentials;
using WalletFramework.Oid4Vc.Oid4Vp;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Persistence;
using WalletFramework.Storage.Database;

namespace WalletFramework.Storage.Tests;

public class CompletedPresentationRecordCrudTests : IDisposable
{
    public CompletedPresentationRecordCrudTests() => (_serviceProvider, _dbPath) = TestDbSetup.CreateServiceProvider();

    private readonly ServiceProvider _serviceProvider;

    private readonly string _dbPath;

    [Fact]
    public async Task Can_Store_And_Retrieve_CompletedPresentationRecord()
    {
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider
            .GetRequiredService<IDomainRepository<CompletedPresentation, CompletedPresentationRecord, string>>();

        var presentation = CreateSamplePresentation();

        await repository.Add(presentation);
        var fetched = await repository.GetById(presentation.PresentationId);

        fetched.Match(
            found =>
            {
                found.PresentationId.Should().Be(presentation.PresentationId);
                found.ClientId.Should().Be(presentation.ClientId);
                found.PresentedCredentialSets.Count.Should().Be(presentation.PresentedCredentialSets.Count);
            },
            () => throw new InvalidOperationException("Record should exist")
        );
    }

    [Fact]
    public async Task Can_Update_CompletedPresentationRecord()
    {
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider
            .GetRequiredService<IDomainRepository<CompletedPresentation, CompletedPresentationRecord, string>>();

        var presentation = CreateSamplePresentation();
        await repository.Add(presentation);

        var updated = presentation with { Name = Prelude.Some("Updated Name") };
        await repository.Update(updated);

        var fetched = await repository.GetById(presentation.PresentationId);

        fetched.Match(
            found =>
            {
                found.Name.Match(
                    Some: n => n.Should().Be("Updated Name"),
                    None: () => Assert.Fail("Name should have a value")
                );
            },
            () => throw new InvalidOperationException("Record should exist")
        );
    }

    [Fact]
    public async Task Can_Delete_CompletedPresentationRecord_By_Id()
    {
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider
            .GetRequiredService<IDomainRepository<CompletedPresentation, CompletedPresentationRecord, string>>();

        var presentation = CreateSamplePresentation();
        await repository.Add(presentation);

        await repository.Delete(presentation.PresentationId);
        var fetched = await repository.GetById(presentation.PresentationId);

        fetched.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task Can_Delete_CompletedPresentationRecord_By_Domain()
    {
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider
            .GetRequiredService<IDomainRepository<CompletedPresentation, CompletedPresentationRecord, string>>();

        var presentation = CreateSamplePresentation();
        await repository.Add(presentation);

        await repository.Delete(presentation);
        var fetched = await repository.GetById(presentation.PresentationId);

        fetched.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task Can_ListAll_CompletedPresentationRecords()
    {
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider
            .GetRequiredService<IDomainRepository<CompletedPresentation, CompletedPresentationRecord, string>>();

        var p1 = CreateSamplePresentation();
        var p2 = CreateSamplePresentation();
        await repository.Add(p1);
        await repository.Add(p2);

        var list = await repository.ListAll();

        list.Match(
            Some: items =>
            {
                items.Count.Should().BeGreaterOrEqualTo(2);
                items.Any(i => i.PresentationId == p1.PresentationId).Should().BeTrue();
                items.Any(i => i.PresentationId == p2.PresentationId).Should().BeTrue();
            },
            None: () => throw new InvalidOperationException("List should not be empty")
        );
    }

    [Fact]
    public async Task Can_Find_CompletedPresentationRecords_By_ClientId()
    {
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider
            .GetRequiredService<IDomainRepository<CompletedPresentation, CompletedPresentationRecord, string>>();

        var clientId = Guid.NewGuid().ToString("N");
        var p1 = CreateSamplePresentation() with { ClientId = clientId };
        var p2 = CreateSamplePresentation() with { ClientId = clientId };
        var other = CreateSamplePresentation();

        await repository.Add(p1);
        await repository.Add(p2);
        await repository.Add(other);

        var cfg = new FindCompletedPresentationsByClientId(clientId);
        var result = await repository.Find(cfg);

        result.Match(
            Some: items =>
            {
                items.Count.Should().Be(2);
                items.All(i => i.ClientId == clientId).Should().BeTrue();
            },
            None: () => throw new InvalidOperationException("Result should not be empty")
        );
    }

    [Fact]
    public async Task RecordConfiguration_Enforces_Unique_PresentationId()
    {
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider
            .GetRequiredService<IDomainRepository<CompletedPresentation, CompletedPresentationRecord, string>>();

        var p1 = CreateSamplePresentation();
        var p2 = p1 with { ClientId = Guid.NewGuid().ToString() };

        await repository.Add(p1);

        Func<Task> act = async () => await repository.Add(p2);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public void Record_Serialization_RoundTrip()
    {
        var presentation = CreateSamplePresentation();
        var record = new CompletedPresentationRecord(presentation);
        var back = record.ToDomainModel();

        back.PresentationId.Should().Be(presentation.PresentationId);
        back.ClientId.Should().Be(presentation.ClientId);
        back.PresentedCredentialSets.Count.Should().Be(presentation.PresentedCredentialSets.Count);
    }

    public void Dispose()
    {
        TestDbSetup.Cleanup(_serviceProvider, _dbPath);
        GC.SuppressFinalize(this);
    }

    private static CompletedPresentation CreateSamplePresentation()
    {
        var presented = new PresentedCredentialSet
        {
            CredentialSetId = CredentialSetId.CreateCredentialSetId(),
            SdJwtCredentialType = Option<SdJwtVc.Models.Vct>.None,
            MdocCredentialType = Option<MdocLib.DocType>.None,
            PresentedClaims = new Dictionary<string, PresentedClaim>
            {
                { "given_name", new PresentedClaim { Value = "Jane" } },
                { "family_name", new PresentedClaim { Value = "Doe" } }
            }
        };

        var clientMetadata = Option<ClientMetadata>.None;
        var name = Option<string>.None;

        return new CompletedPresentation(
            Guid.NewGuid().ToString("N"),
            Guid.NewGuid().ToString("N"),
            [presented],
            clientMetadata,
            name,
            DateTimeOffset.UtcNow);
    }

    private sealed record FindCompletedPresentationsByClientId(string ClientId)
        : ISearchConfig<CompletedPresentationRecord>
    {
        public Expression<Func<CompletedPresentationRecord, bool>> ToPredicate()
        {
            return record => record.ClientId == ClientId;
        }
    }
}
