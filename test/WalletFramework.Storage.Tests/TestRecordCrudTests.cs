using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using WalletFramework.Core.Functional;
using WalletFramework.Storage.Database;
using WalletFramework.Storage.Repositories;
using WalletFramework.Storage.Tests.TestModels;

namespace WalletFramework.Storage.Tests;

public class TestRecordCrudTests : IDisposable
{
    public TestRecordCrudTests()
    {
        (_serviceProvider, _dbPath) = TestDbSetup.CreateServiceProvider();
    }

    private readonly ServiceProvider _serviceProvider;

    private readonly string _dbPath;

    [Fact]
    public async Task Add_Should_Successfully_Create_Record()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();
        var testRecord = TestRecord.Create("Test Name", "Test Description", 42, true);

        // Act
        var result = await repository.Add(testRecord);

        // Assert
        result.Should().Be(Unit.Default);

        var retrieved = await repository.GetById(testRecord.RecordId);
        retrieved.IsSome.Should().BeTrue();

        var record = retrieved.IfNone(() => throw new InvalidOperationException("Record should exist"));
        record.Name.Should().Be("Test Name");
        record.Description.Should().Be("Test Description");
        record.Value.Should().Be(42);
        record.IsActive.Should().BeTrue();
        record.RecordId.Should().Be(testRecord.RecordId);
    }

    public void Dispose()
    {
        TestDbSetup.Cleanup(_serviceProvider, _dbPath);
    }

    [Fact]
    public async Task Find_Should_Return_Matching_Records()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();

        var record1 = TestRecord.Create("Active Record", "Description 1", 10, true);
        var record2 = TestRecord.Create("Inactive Record", "Description 2", 20, false);
        var record3 = TestRecord.Create("Another Active", "Description 3", 30, true);

        await repository.Add(record1);
        await repository.Add(record2);
        await repository.Add(record3);

        // Act
        var activeRecords = (await repository.Find(r => r.IsActive)).UnwrapOrThrow();
        var recordsWithValue20 = (await repository.Find(r => r.Value == 20)).UnwrapOrThrow();

        // Assert
        activeRecords.Should().HaveCount(2);
        activeRecords.Should().Contain(r => r.RecordId == record1.RecordId);
        activeRecords.Should().Contain(r => r.RecordId == record3.RecordId);

        recordsWithValue20.Should().HaveCount(1);
        recordsWithValue20.Should().Contain(r => r.RecordId == record2.RecordId);
    }

    [Fact]
    public async Task GetById_Should_Return_None_When_Record_Does_Not_Exist()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetById(nonExistentId);

        // Assert
        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task GetById_Should_Return_Record_When_Exists()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();
        var testRecord = TestRecord.Create("Test Name", "Test Description", 42, true);
        await repository.Add(testRecord);

        // Act
        var result = await repository.GetById(testRecord.RecordId);

        // Assert
        result.IsSome.Should().BeTrue();
        var record = result.IfNone(() => throw new InvalidOperationException("Record should exist"));
        record.RecordId.Should().Be(testRecord.RecordId);
        record.Name.Should().Be("Test Name");
    }

    [Fact]
    public async Task ListAll_Should_Return_All_Records()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();

        var record1 = TestRecord.Create("Record 1", "Description 1", 10, true);
        var record2 = TestRecord.Create("Record 2", "Description 2", 20, false);
        var record3 = TestRecord.Create("Record 3", "Description 3", 30, true);

        await repository.Add(record1);
        await repository.Add(record2);
        await repository.Add(record3);

        // Act
        var allRecords = (await repository.ListAll()).UnwrapOrThrow();

        // Assert
        allRecords.Should().HaveCount(3);
        allRecords.Should().Contain(r => r.RecordId == record1.RecordId);
        allRecords.Should().Contain(r => r.RecordId == record2.RecordId);
        allRecords.Should().Contain(r => r.RecordId == record3.RecordId);
    }

    [Fact]
    public async Task Multiple_Operations_Should_Work_Together()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();

        // Act & Assert - Create multiple records
        var record1 = TestRecord.Create("Record 1", "First record", 1, true);
        var record2 = TestRecord.Create("Record 2", "Second record", 2, false);
        var record3 = TestRecord.Create("Record 3", "Third record", 3, true);

        await repository.Add(record1);
        await repository.Add(record2);
        await repository.Add(record3);

        var allRecords = (await repository.ListAll()).UnwrapOrThrow();
        allRecords.Should().HaveCount(3);

        // Update one record
        var updatedRecord2 = record2.Update(isActive: true, value: 22);
        await repository.Update(updatedRecord2);

        var activeRecords = (await repository.Find(r => r.IsActive)).UnwrapOrThrow();
        activeRecords.Should().HaveCount(3); // All should be active now

        // Delete one record
        await repository.RemoveById(record1.RecordId);

        var remainingRecords = (await repository.ListAll()).UnwrapOrThrow();
        remainingRecords.Should().HaveCount(2);
        remainingRecords.Should().NotContain(r => r.RecordId == record1.RecordId);
    }

    [Fact]
    public async Task Remove_Should_Delete_Record()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();
        var testRecord = TestRecord.Create("To Be Deleted", "Description", 999, true);
        await repository.Add(testRecord);

        // Verify record exists before deletion
        var beforeDeletion = await repository.GetById(testRecord.RecordId);
        beforeDeletion.IsSome.Should().BeTrue();

        // Act
        await repository.Remove(testRecord);

        // Assert
        var afterDeletion = await repository.GetById(testRecord.RecordId);
        afterDeletion.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveById_Should_Delete_Record_By_Id()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();
        var testRecord = TestRecord.Create("To Be Deleted By Id", "Description", 888, false);
        await repository.Add(testRecord);

        // Verify record exists before deletion
        var beforeDeletion = await repository.GetById(testRecord.RecordId);
        beforeDeletion.IsSome.Should().BeTrue();

        // Act
        await repository.RemoveById(testRecord.RecordId);

        // Assert
        var afterDeletion = await repository.GetById(testRecord.RecordId);
        afterDeletion.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveById_Should_Handle_Non_Existent_Id_Gracefully()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert - Should not throw
        var result = await repository.RemoveById(nonExistentId);
        result.Should().Be(Unit.Default);
    }

    [Fact]
    public async Task Update_Should_Modify_Existing_Record()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();
        var originalRecord = TestRecord.Create("Original Name", "Original Description", 100, false);
        await repository.Add(originalRecord);

        // Act
        var updatedRecord = originalRecord.Update(name: "Updated Name", value: 200, isActive: true);
        await repository.Update(updatedRecord);

        // Assert
        var retrieved = await repository.GetById(originalRecord.RecordId);
        retrieved.IsSome.Should().BeTrue();

        var record = retrieved.IfNone(() => throw new InvalidOperationException("Record should exist"));
        record.Name.Should().Be("Updated Name");
        record.Description.Should().Be("Original Description"); // Should remain unchanged
        record.Value.Should().Be(200);
        record.IsActive.Should().BeTrue();
        record.UpdatedAt.Should().BeAfter(record.CreatedAt);
    }

    [Fact]
    public async Task Can_Store_And_Retrieve_Tags()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();
        var testRecord = TestRecord.Create("Test", "Description", 1, true);

        // Act - Set tags
        testRecord.SetTag("Category", "TestCategory", s => s);
        testRecord.SetTag("Priority", "High", s => s);

        await repository.Add(testRecord);

        // Assert - Retrieve and verify tags
        var retrieved = await repository.GetById(testRecord.RecordId);
        _ = retrieved.Match(
            record =>
            {
                record.GetTag("Category", s => s).UnwrapOrThrow().Should().Be("TestCategory");
                record.GetTag("Priority", s => s).UnwrapOrThrow().Should().Be("High");
            },
            () => throw new InvalidOperationException("Record should exist")
        );
    }

    [Fact]
    public async Task Can_Update_Tag()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();
        var testRecord = TestRecord.Create("Test", "Description", 1, true);

        // Act - Set initial value then update
        testRecord.SetTag("Status", "Initial", s => s);
        testRecord.SetTag("Status", "Updated", s => s);

        await repository.Add(testRecord);

        // Assert - Should have updated value
        var retrieved = await repository.GetById(testRecord.RecordId);
        _ = retrieved.Match(
            record => record.GetTag("Status", s => s).UnwrapOrThrow().Should().Be("Updated"),
            () => throw new InvalidOperationException("Record should exist")
        );
    }

    [Fact]
    public async Task Can_Not_Get_Non_Existent_Tag()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();
        var testRecord = TestRecord.Create("Test", "Description", 1, true);
        testRecord.SetTag("ExistingTag", "value", s => s);

        await repository.Add(testRecord);

        // Act & Assert - Should return null when accessing non-existent tag
        var retrieved = await repository.GetById(testRecord.RecordId);
        _ = retrieved.Match(
            record =>
            {
                var nonExistentTag = record.GetTag("NonExistentTag", s => s);
                nonExistentTag.IsNone.Should().BeTrue();
            },
            () => throw new InvalidOperationException("Record should exist")
        );
    }

    [Fact]
    public async Task Can_Remove_Tag()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();
        var testRecord = TestRecord.Create("Test", "Description", 1, true);

        // Set a tag
        testRecord.SetTag("TestTag", "testValue", s => s);

        await repository.Add(testRecord);

        // Act - Remove the tag
        var retrieved = await repository.GetById(testRecord.RecordId);
        var record = retrieved.IfNone(() => throw new InvalidOperationException("Record should exist"));
        record.RemoveTag("TestTag");

        await repository.Update(record);

        // Assert - Tag should be removed
        var updatedRecord = await repository.GetById(testRecord.RecordId);
        _ = updatedRecord.Match(
            r => r.GetTag("TestTag", s => s).IsNone.Should().BeTrue(),
            () => throw new InvalidOperationException("Record should exist")
        );
    }

    [Fact]
    public async Task Can_Store_And_Retrieve_Complex_Object_With_Serialization()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();
        var testRecord = TestRecord.Create("Test", "Description", 1, true);

        // Define serialization/deserialization functions
        var serializeFunc = (DateTime dt) => dt.ToString("O"); // ISO 8601 format
        var deserializeFunc = (string s) => DateTime.Parse(s);

        var testDate = new DateTime(2023, 12, 25, 10, 30, 0);

        // Act - Set complex object with serialization
        testRecord.SetTag("LastUpdated", testDate, serializeFunc);

        await repository.Add(testRecord);

        // Assert - Retrieve and verify with deserialization
        var retrieved = await repository.GetById(testRecord.RecordId);
        _ = retrieved.Match(
            record =>
            {
                record.GetTag("LastUpdated", deserializeFunc).UnwrapOrThrow().Should().Be(testDate);
            },
            () => throw new InvalidOperationException("Record should exist")
        );
    }

    [Fact]
    public async Task Can_Store_And_Retrieve_Object_With_JSON_Serialization()
    {
        // Arrange
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider.GetRequiredService<IRepository<TestRecord>>();
        var testRecord = TestRecord.Create("Test", "Description", 1, true);

        // Define JSON serialization/deserialization functions
        var serializeFunc = (Dictionary<string, int> dict) => JsonSerializer.Serialize(dict);
        var deserializeFunc = (string json) => JsonSerializer.Deserialize<Dictionary<string, int>>(json)!;

        var testData = new Dictionary<string, int>
        {
            ["apples"] = 5,
            ["bananas"] = 3,
            ["oranges"] = 7
        };

        // Act - Set complex object with JSON serialization
        testRecord.SetTag("Inventory", testData, serializeFunc);

        await repository.Add(testRecord);

        // Assert - Retrieve and verify with JSON deserialization
        var retrieved = await repository.GetById(testRecord.RecordId);
        _ = retrieved.Match(
            record =>
            {
                var option = record.GetTag("Inventory", deserializeFunc);
                _ = option.Match(
                    data => data.Should().BeEquivalentTo(testData),
                    () => throw new InvalidOperationException("Record should exist")
                );
            },
            () => throw new InvalidOperationException("Record should exist")
        );
    }
}
