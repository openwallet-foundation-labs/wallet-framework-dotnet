using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib;
using WalletFramework.MdocVc;
using WalletFramework.MdocVc.Persistence;
using WalletFramework.Storage.Database;
using WalletFramework.Storage;
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
        using var scope = _serviceProvider.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var databaseCreator = serviceProvider.GetRequiredService<IDatabaseCreator>();
        var storageSession = serviceProvider.GetRequiredService<IStorageSession>();
        await databaseCreator.EnsureDatabaseCreated();

        var store = serviceProvider.GetRequiredService<IMdocCredentialStore>();

        var encodedMdoc = MdocSamples.GetEncodedMdocSample();
        var mdoc = Mdoc.ValidMdoc(encodedMdoc).UnwrapOrThrow();
        var mdocCredential = mdoc.ToMdocCredential(
            KeyId.CreateKeyId(),
            CredentialSetId.CreateCredentialSetId(),
            CredentialState.Active,
            false,
            Option<DateTime>.None,
            CredentialId.CreateCredentialId());

        await store.Add(mdocCredential);
        await storageSession.Commit();

        var fetched = await store.Get(mdocCredential.CredentialId);

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
        using var scope = _serviceProvider.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var databaseCreator = serviceProvider.GetRequiredService<IDatabaseCreator>();
        var storageSession = serviceProvider.GetRequiredService<IStorageSession>();
        await databaseCreator.EnsureDatabaseCreated();

        var store = serviceProvider.GetRequiredService<IMdocCredentialStore>();

        var encodedMdoc = MdocSamples.GetEncodedMdocSample();
        var mdoc = Mdoc.ValidMdoc(encodedMdoc).UnwrapOrThrow();

        var credentialId = CredentialId.CreateCredentialId();
        var credentialSetId = CredentialSetId.CreateCredentialSetId();
        var keyId = KeyId.CreateKeyId();

        var initial = mdoc.ToMdocCredential(
            keyId,
            credentialSetId,
            CredentialState.Active,
            false,
            Option<DateTime>.None,
            credentialId);

        await store.Add(initial);
        await storageSession.Commit();

        var updated = mdoc.ToMdocCredential(
            keyId,
            credentialSetId,
            CredentialState.Revoked,
            true,
            Option<DateTime>.Some(DateTime.UtcNow.AddDays(1)),
            credentialId);

        await store.Update(updated);
        await storageSession.Commit();

        var fetched = await store.Get(credentialId);

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
        using var scope = _serviceProvider.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var databaseCreator = serviceProvider.GetRequiredService<IDatabaseCreator>();
        var storageSession = serviceProvider.GetRequiredService<IStorageSession>();
        await databaseCreator.EnsureDatabaseCreated();

        var store = serviceProvider.GetRequiredService<IMdocCredentialStore>();

        var encodedMdoc = MdocSamples.GetEncodedMdocSample();
        var mdoc = Mdoc.ValidMdoc(encodedMdoc).UnwrapOrThrow();

        var credentialId = CredentialId.CreateCredentialId();
        var credentialSetId = CredentialSetId.CreateCredentialSetId();
        var keyId = KeyId.CreateKeyId();

        var mdocCredential = mdoc.ToMdocCredential(
            keyId,
            credentialSetId,
            CredentialState.Active,
            false,
            Option<DateTime>.None,
            credentialId);

        await store.Add(mdocCredential);
        await storageSession.Commit();

        await store.Delete(credentialId);
        await storageSession.Commit();

        var fetched = await store.Get(credentialId);

        fetched.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task Can_ListAll_MdocCredentialRecords()
    {
        using var scope = _serviceProvider.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var databaseCreator = serviceProvider.GetRequiredService<IDatabaseCreator>();
        var storageSession = serviceProvider.GetRequiredService<IStorageSession>();
        await databaseCreator.EnsureDatabaseCreated();

        var store = serviceProvider.GetRequiredService<IMdocCredentialStore>();

        var encodedMdoc = MdocSamples.GetEncodedMdocSample();
        var mdoc = Mdoc.ValidMdoc(encodedMdoc).UnwrapOrThrow();

        var id1 = CredentialId.CreateCredentialId();
        var id2 = CredentialId.CreateCredentialId();

        var csid = CredentialSetId.CreateCredentialSetId();
        var kid = KeyId.CreateKeyId();

        var cred1 = mdoc.ToMdocCredential(kid, csid, CredentialState.Active, false, Option<DateTime>.None, id1);
        var cred2 = mdoc.ToMdocCredential(kid, csid, CredentialState.Active, false, Option<DateTime>.None, id2);

        await store.Add(cred1);
        await store.Add(cred2);
        await storageSession.Commit();

        var list = await store.List();

        list.Count.Should().BeGreaterOrEqualTo(2);
        list.Any(c => c.CredentialId.Equals(id1)).Should().BeTrue();
        list.Any(c => c.CredentialId.Equals(id2)).Should().BeTrue();
    }

    [Fact]
    public async Task Can_Find_MdocCredentialRecords_By_DocType()
    {
        using var scope = _serviceProvider.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var databaseCreator = serviceProvider.GetRequiredService<IDatabaseCreator>();
        var storageSession = serviceProvider.GetRequiredService<IStorageSession>();
        await databaseCreator.EnsureDatabaseCreated();

        var store = serviceProvider.GetRequiredService<IMdocCredentialStore>();

        var encodedMdoc = MdocSamples.GetEncodedMdocSample();
        var mdoc = Mdoc.ValidMdoc(encodedMdoc).UnwrapOrThrow();

        var csid = CredentialSetId.CreateCredentialSetId();
        var kid = KeyId.CreateKeyId();

        var cred1 = mdoc.ToMdocCredential(kid, csid, CredentialState.Active, false, Option<DateTime>.None, CredentialId.CreateCredentialId());
        var cred2 = mdoc.ToMdocCredential(kid, csid, CredentialState.Active, false, Option<DateTime>.None, CredentialId.CreateCredentialId());

        await store.Add(cred1);
        await store.Add(cred2);
        await storageSession.Commit();

        var result = await store.ListByDocType(mdoc.DocType);

        result.Count.Should().Be(2);
        result.All(i => i.Mdoc.DocType.AsString() == mdoc.DocType.AsString()).Should().BeTrue();
    }

    [Fact]
    public async Task Can_Find_MdocCredentialRecords_By_NonExistent_DocType_Returns_Empty()
    {
        using var scope = _serviceProvider.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var databaseCreator = serviceProvider.GetRequiredService<IDatabaseCreator>();
        var storageSession = serviceProvider.GetRequiredService<IStorageSession>();
        await databaseCreator.EnsureDatabaseCreated();

        var store = serviceProvider.GetRequiredService<IMdocCredentialStore>();

        var encodedMdoc = MdocSamples.GetEncodedMdocSample();
        var mdoc = Mdoc.ValidMdoc(encodedMdoc).UnwrapOrThrow();

        var csid = CredentialSetId.CreateCredentialSetId();
        var kid = KeyId.CreateKeyId();

        var cred1 = mdoc.ToMdocCredential(kid, csid, CredentialState.Active, false, Option<DateTime>.None, CredentialId.CreateCredentialId());

        await store.Add(cred1);
        await storageSession.Commit();

        var nonExistentDocType = DocType.ValidDoctype(new JValue("non.existent.doctype")).UnwrapOrThrow();
        var result = await store.ListByDocType(nonExistentDocType);

        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        TestDbSetup.Cleanup(_serviceProvider, _dbPath);
        GC.SuppressFinalize(this);
    }
}
