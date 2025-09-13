using WalletFramework.Storage.Records;

namespace WalletFramework.Storage.Tests.TestModels;

/// <summary>
///     Test record for verifying CRUD operations on BaseRecord implementations.
/// </summary>
public record TestRecord() : RecordBase(Guid.NewGuid())
{
    /// <summary>
    ///     Gets the boolean flag for testing.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    ///     Gets the integer value for testing.
    /// </summary>
    public required int Value { get; init; }

    /// <summary>
    ///     Gets the description of the test record.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    ///     Gets the name of the test record.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Factory method to create a new TestRecord.
    /// </summary>
    /// <param name="name">The name of the record.</param>
    /// <param name="description">The description of the record.</param>
    /// <param name="value">The integer value.</param>
    /// <param name="isActive">The boolean flag.</param>
    /// <returns>A new TestRecord instance.</returns>
    public static TestRecord Create(string name, string description, int value, bool isActive)
    {
        var now = DateTimeOffset.UtcNow;
        return new TestRecord
        {
            Name = name,
            Description = description,
            Value = value,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now,
            RecordId = Guid.NewGuid()
        };
    }

    /// <summary>
    ///     Factory method to create a TestRecord with specific Id and timestamps.
    /// </summary>
    /// <param name="id">The record identifier.</param>
    /// <param name="name">The name of the record.</param>
    /// <param name="description">The description of the record.</param>
    /// <param name="value">The integer value.</param>
    /// <param name="isActive">The boolean flag.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <param name="updatedAt">The update timestamp.</param>
    /// <returns>A new TestRecord instance.</returns>
    public static TestRecord Create(string id, string name, string description, int value, bool isActive,
        DateTimeOffset createdAt, DateTimeOffset updatedAt) =>
        new()
        {
            RecordId = Guid.Parse(id),
            Name = name,
            Description = description,
            Value = value,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

    /// <summary>
    ///     Creates a copy of this record with updated properties.
    /// </summary>
    /// <param name="name">The new name (optional).</param>
    /// <param name="description">The new description (optional).</param>
    /// <param name="value">The new value (optional).</param>
    /// <param name="isActive">The new active flag (optional).</param>
    /// <returns>A new TestRecord instance with updated properties.</returns>
    public TestRecord Update(string? name = null, string? description = null, int? value = null,
        bool? isActive = null) =>
        this with
        {
            Name = name ?? Name,
            Description = description ?? Description,
            Value = value ?? Value,
            IsActive = isActive ?? IsActive,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}
