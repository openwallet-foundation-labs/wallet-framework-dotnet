using System.ComponentModel.DataAnnotations;
using LanguageExt;

namespace WalletFramework.Storage.Records;

/// <summary>
///     Base record class that provides common properties for all storage entities.
/// </summary>
public abstract record RecordBase
{
    /// <summary>
    ///     Gets or initializes the timestamp when the record was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     Gets or initializes the timestamp when the record was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    ///     Gets or initializes the unique identifier for the record.
    /// </summary>
    [Key]
    public Guid RecordId { get; init; }

    public Dictionary<string, string> Tags { get; init; }

    /// <summary>
    ///     Constructor that allows setting a custom Id.
    /// </summary>
    /// <param name="recordId">The unique identifier for the record.</param>
    protected RecordBase(Guid recordId)
    {
        var now = DateTimeOffset.UtcNow;

        RecordId = recordId;
        CreatedAt = now;
        UpdatedAt = now;
        Tags = [];
    }

    public Option<T> GetTag<T>(string key, Func<string, T> deserialize)
    {
        if (!Tags.TryGetValue(key, out var value))
            return Option<T>.None;

        return deserialize(value);
    }

    public Unit SetTag<T>(string key, T value, Func<T, string> serialize)
    {
        var stringValue = serialize(value);
        Tags[key] = stringValue;
        return Unit.Default;
    }

    public Unit RemoveTag(string key)
    {
        Tags.Remove(key);
        return Unit.Default;
    }
}
