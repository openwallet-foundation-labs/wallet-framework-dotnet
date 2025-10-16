using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib;
using WalletFramework.MdocVc;
using WalletFramework.MdocVc.Display;
using WalletFramework.MdocVc.Persistence;
using WalletFramework.Storage.Database;
using WalletFramework.TestSamples;

namespace WalletFramework.Storage.Tests;

public class MdocCredentialRecordCrudTests : IDisposable
{
    public MdocCredentialRecordCrudTests() => (_serviceProvider, _dbPath) = TestDbSetup.CreateServiceProvider();

    private readonly ServiceProvider _serviceProvider;

    private readonly string _dbPath;

    [Fact]
    public async Task Can_Store_And_Retrieve_MdocCredentialRecord()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId>>();

        var encodedMdoc = MdocSamples.GetEncodedMdocSample();

        var mdoc = Mdoc.ValidMdoc(encodedMdoc).UnwrapOrThrow();

        var mdocCredential = mdoc.ToMdocCredential(
            Option<List<MdocDisplay>>.None,
            KeyId.CreateKeyId(),
            CredentialSetId.CreateCredentialSetId(),
            CredentialState.Active,
            false,
            Option<DateTime>.None,
            CredentialId.CreateCredentialId());

        // Act
        await repository.Add(mdocCredential);
        var fetched = await repository.GetById(mdocCredential.CredentialId);

        // Assert
        fetched.Match(
            found =>
            {
                found.CredentialId.Should().Be(mdocCredential.CredentialId);
                found.Mdoc.DocType.Should().Be(mdocCredential.Mdoc.DocType);
                found.CredentialId.AsString().Should().Be(mdocCredential.CredentialId.AsString());
                found.CredentialSetId.AsString().Should().Be(mdocCredential.CredentialSetId.AsString());
                found.KeyId.AsString().Should().Be(mdocCredential.KeyId.AsString());
            },
            () => throw new InvalidOperationException("Record should exist")
        );
    }

    [Fact]
    public async Task Can_Update_MdocCredentialRecord()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId>>();

        var encodedMdoc = MdocSamples.GetEncodedMdocSample();
        var mdoc = Mdoc.ValidMdoc(encodedMdoc).UnwrapOrThrow();

        var credentialId = CredentialId.CreateCredentialId();
        var credentialSetId = CredentialSetId.CreateCredentialSetId();
        var keyId = KeyId.CreateKeyId();

        var initial = mdoc.ToMdocCredential(
            Option<List<MdocDisplay>>.None,
            keyId,
            credentialSetId,
            CredentialState.Active,
            false,
            Option<DateTime>.None,
            credentialId);

        await repository.Add(initial);

        // Act
        var updated = mdoc.ToMdocCredential(
            Option<List<MdocDisplay>>.None,
            keyId,
            credentialSetId,
            CredentialState.Revoked,
            true,
            Option<DateTime>.Some(DateTime.UtcNow.AddDays(1)),
            credentialId);

        await repository.Update(updated);

        var fetched = await repository.GetById(credentialId);

        // Assert
        fetched.Match(
            found =>
            {
                found.CredentialId.Should().Be(credentialId);
                found.CredentialState.Should().Be(CredentialState.Revoked);
                found.OneTimeUse.Should().BeTrue();
                found.ExpiresAt.IsSome.Should().BeTrue();
            },
            () => throw new InvalidOperationException("Record should exist")
        );
    }

    [Fact]
    public async Task Can_Delete_MdocCredentialRecord()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId>>();

        var encodedMdoc = MdocSamples.GetEncodedMdocSample();
        var mdoc = Mdoc.ValidMdoc(encodedMdoc).UnwrapOrThrow();

        var credentialId = CredentialId.CreateCredentialId();
        var credentialSetId = CredentialSetId.CreateCredentialSetId();
        var keyId = KeyId.CreateKeyId();

        var mdocCredential = mdoc.ToMdocCredential(
            Option<List<MdocDisplay>>.None,
            keyId,
            credentialSetId,
            CredentialState.Active,
            false,
            Option<DateTime>.None,
            credentialId);

        await repository.Add(mdocCredential);

        // Act
        // Delete by id
        await repository.Delete(credentialId);
        var fetched = await repository.GetById(credentialId);

        // Assert
        fetched.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task Can_Delete_MdocCredentialRecord_By_Domain()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId>>();

        var encodedMdoc = MdocSamples.GetEncodedMdocSample();
        var mdoc = Mdoc.ValidMdoc(encodedMdoc).UnwrapOrThrow();

        var credentialId = CredentialId.CreateCredentialId();
        var credentialSetId = CredentialSetId.CreateCredentialSetId();
        var keyId = KeyId.CreateKeyId();

        var mdocCredential = mdoc.ToMdocCredential(
            Option<List<MdocDisplay>>.None,
            keyId,
            credentialSetId,
            CredentialState.Active,
            false,
            Option<DateTime>.None,
            credentialId);

        await repository.Add(mdocCredential);

        // Act: delete by domain instance
        await repository.Delete(mdocCredential);
        var fetched = await repository.GetById(credentialId);

        // Assert
        fetched.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task Can_ListAll_MdocCredentialRecords()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId>>();

        var encodedMdoc = MdocSamples.GetEncodedMdocSample();
        var mdoc = Mdoc.ValidMdoc(encodedMdoc).UnwrapOrThrow();

        var id1 = CredentialId.CreateCredentialId();
        var id2 = CredentialId.CreateCredentialId();

        var csid = CredentialSetId.CreateCredentialSetId();
        var kid = KeyId.CreateKeyId();

        var cred1 = mdoc.ToMdocCredential(Option<List<MdocDisplay>>.None, kid, csid, CredentialState.Active, false, Option<DateTime>.None, id1);
        var cred2 = mdoc.ToMdocCredential(Option<List<MdocDisplay>>.None, kid, csid, CredentialState.Active, false, Option<DateTime>.None, id2);

        await repository.Add(cred1);
        await repository.Add(cred2);

        // Act
        var list = await repository.ListAll();

        // Assert
        list.Match(
            Some: items =>
            {
                items.Count.Should().BeGreaterOrEqualTo(2);
                items.Any(c => c.CredentialId.Equals(id1)).Should().BeTrue();
                items.Any(c => c.CredentialId.Equals(id2)).Should().BeTrue();
            },
            None: () => throw new InvalidOperationException("List should not be empty")
        );
    }

    [Fact]
    public async Task Can_Find_MdocCredentialRecords_By_DocType()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId>>();

        var encodedMdoc = MdocSamples.GetEncodedMdocSample();
        var mdoc = Mdoc.ValidMdoc(encodedMdoc).UnwrapOrThrow();

        var csid = CredentialSetId.CreateCredentialSetId();
        var kid = KeyId.CreateKeyId();

        var cred1 = mdoc.ToMdocCredential(Option<List<MdocDisplay>>.None, kid, csid, CredentialState.Active, false, Option<DateTime>.None, CredentialId.CreateCredentialId());
        var cred2 = mdoc.ToMdocCredential(Option<List<MdocDisplay>>.None, kid, csid, CredentialState.Active, false, Option<DateTime>.None, CredentialId.CreateCredentialId());

        await repository.Add(cred1);
        await repository.Add(cred2);

        // Act
        var cfg = new FindMdocCredentialsWithDocType { DocType = mdoc.DocType };
        var result = await repository.Find(cfg);

        // Assert
        result.Match(
            Some: items =>
            {
                items.Count.Should().Be(2);
                items.All(i => i.Mdoc.DocType.AsString() == mdoc.DocType.AsString()).Should().BeTrue();
            },
            None: () => throw new InvalidOperationException("Result should not be empty")
        );
    }

    [Fact]
    public async Task Can_Find_MdocCredentialRecords_By_NonExistent_DocType_Returns_Empty()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId>>();

        var encodedMdoc = MdocSamples.GetEncodedMdocSample();
        var mdoc = Mdoc.ValidMdoc(encodedMdoc).UnwrapOrThrow();

        var csid = CredentialSetId.CreateCredentialSetId();
        var kid = KeyId.CreateKeyId();

        var cred1 = mdoc.ToMdocCredential(Option<List<MdocDisplay>>.None, kid, csid, CredentialState.Active, false, Option<DateTime>.None, CredentialId.CreateCredentialId());

        await repository.Add(cred1);

        // Act - search for a non-existent DocType
        var nonExistentDocType = DocType.ValidDoctype(new JValue("non.existent.doctype")).UnwrapOrThrow();
        var cfg = new FindMdocCredentialsWithDocType { DocType = nonExistentDocType };
        var result = await repository.Find(cfg);

        // Assert
        result.Match(
            Some: _ =>
            {
                Assert.Fail("Result should be empty for non-existent DocType");
            },
            None: () =>
            {
            }
        );
    }

    public void Dispose()
    {
        TestDbSetup.Cleanup(_serviceProvider, _dbPath);
        GC.SuppressFinalize(this);
    }
}
