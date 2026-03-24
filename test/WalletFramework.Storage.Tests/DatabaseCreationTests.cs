using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WalletFramework.Storage;
using WalletFramework.Storage.Database;
using WalletFramework.Storage.Repositories;
using WalletFramework.Storage.Tests.TestModels;

namespace WalletFramework.Storage.Tests;

public sealed class DatabaseCreationTests : IDisposable
{
    private const int ConcurrentWorkers = 24;
    private const int SequentialEnsureCalls = 5;

    public DatabaseCreationTests()
    {
        (_serviceProvider, _dbPath) = TestDbSetup.CreateServiceProvider();
        _scope = _serviceProvider.CreateScope();
        _databaseCreator = _scope.ServiceProvider.GetRequiredService<IDatabaseCreator>();
        _repository = _scope.ServiceProvider.GetRequiredService<IRepository<TestRecord>>();
        _storageSession = _scope.ServiceProvider.GetRequiredService<IStorageSession>();
    }

    private readonly ServiceProvider _serviceProvider;
    private readonly string _dbPath;
    private readonly IServiceScope _scope;
    private readonly IDatabaseCreator _databaseCreator;
    private readonly IRepository<TestRecord> _repository;
    private readonly IStorageSession _storageSession;

    [Fact]
    public async Task Can_Create_Database()
    {
        var record = CreateTestRecord(1);
        
        await EnsureDatabaseCreated();
        await StoreRecord(record);
        
        File.Exists(_dbPath).Should().BeTrue("creating the database should produce the sqlite file");
        await AssertStoredRecordCanBeRetrieved(record);
    }

    public void Dispose()
    {
        _scope.Dispose();
        TestDbSetup.Cleanup(_serviceProvider, _dbPath);
    }

    [Fact]
    public async Task Multiple_Concurrent_Calls_Result_In_One_Database()
    {
        var record = CreateTestRecord(2);
        
        var exceptions = await RunConcurrentInitializationAttempt();
        await StoreRecord(record);

        exceptions.Should().BeEmpty("concurrent creation attempts should initialize the database once without errors");
        File.Exists(_dbPath).Should().BeTrue("concurrent creation should still produce a single sqlite database file");
        await AssertStoredRecordCanBeRetrieved(record);
        await AssertPersistedRecordCount(1);
    }

    [Fact]
    public async Task Multiple_Sequential_Calls_Result_In_One_Database()
    {
        var record = CreateTestRecord(3);
        
        foreach (var _ in Enumerable.Range(0, SequentialEnsureCalls))
        {
            await EnsureDatabaseCreated();
        }
        await StoreRecord(record);

        File.Exists(_dbPath)
            .Should()
            .BeTrue("repeated creation attempts should keep using the same sqlite database file");
        await AssertStoredRecordCanBeRetrieved(record);
        await AssertPersistedRecordCount(1);
    }

    private async Task AssertPersistedRecordCount(int expectedCount)
    {
        var records = await _repository.ListAll();
        _ = records.Match(
            Some: persistedRecords => persistedRecords.Should().HaveCount(expectedCount),
            None: () => throw new InvalidOperationException($"Expected {expectedCount} persisted records to exist."));
    }

    private async Task AssertStoredRecordCanBeRetrieved(TestRecord expectedRecord)
    {
        var retrievedRecord = await _repository.GetById(expectedRecord.RecordId);
        retrievedRecord
            .IsSome
            .Should()
            .BeTrue("a stored record should be retrievable after the database has been created");

        var storedRecord = retrievedRecord
            .IfNone(() => throw new InvalidOperationException("Expected stored record to exist."));
        storedRecord.RecordId.Should().Be(expectedRecord.RecordId);
        storedRecord.Name.Should().Be(expectedRecord.Name);
        storedRecord.Description.Should().Be(expectedRecord.Description);
        storedRecord.Value.Should().Be(expectedRecord.Value);
        storedRecord.IsActive.Should().Be(expectedRecord.IsActive);
    }

    private static TaskCompletionSource<bool> CreateStartSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static TestRecord CreateTestRecord(int index) =>
        TestRecord.Create(
            $"Database Creation Record {index}",
            $"Database Creation Description {index}",
            index,
            index % 2 == 0);

    private async Task EnsureDatabaseCreated()
    {
        await _databaseCreator.EnsureDatabaseCreated();
    }

    private static async Task RunAndCapture(ConcurrentBag<Exception> observedExceptions, Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            observedExceptions.Add(exception);
        }
    }

    private async Task<IReadOnlyList<Exception>> RunConcurrentInitializationAttempt()
    {
        var startSignal = CreateStartSignal();
        var observedExceptions = new ConcurrentBag<Exception>();

        var workers = Enumerable
            .Range(0, ConcurrentWorkers)
            .Select(_ => Task.Run(async () =>
            {
                await startSignal.Task;
                await RunAndCapture(
                    observedExceptions,
                    async () =>
                    {
                        await _databaseCreator.EnsureDatabaseCreated();
                    });
            }))
            .ToArray();

        startSignal.SetResult(true);
        await Task.WhenAll(workers);

        return observedExceptions.ToArray();
    }

    private async Task StoreRecord(TestRecord record)
    {
        await _repository.Add(record);
        await _storageSession.Commit();
    }
}
