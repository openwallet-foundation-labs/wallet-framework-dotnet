using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace WalletFramework.Storage.Records;

/// <summary>
/// Entity Framework configuration for the BaseRecord class.
/// This sets up how records are stored in the database.
/// </summary>
public record RecordBaseConfiguration : IRecordConfiguration<RecordBase>
{
    public Unit Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<RecordBase>();

        entity.Property(r => r.RecordId).HasConversion<Guid>();
        entity.HasKey(r => r.RecordId);
        entity.HasIndex(r => r.RecordId);

        ConfigureTagsProperty(entity);

        return Unit.Default;
    }

    /// <summary>
    /// Configures how the Tags dictionary is stored and compared in the database.
    /// </summary>
    private static void ConfigureTagsProperty(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<RecordBase> entity)
    {
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var tagsConverter = new ValueConverter<Dictionary<string, string>, string>(
            tags => ConvertTagsToJson(tags),
            json => ConvertJsonToTags(json));

        var tagsComparer = new ValueComparer<Dictionary<string, string>>(
            (tags1, tags2) => AreTagsEqual(tags1, tags2),
            tags => GetTagsHashCode(tags),
            tags => CreateTagsSnapshot(tags));

        entity.Property(r => r.Tags)
            .HasConversion(tagsConverter, tagsComparer)
            .HasColumnType("TEXT");
    }

    /// <summary>
    /// Converts a dictionary of tags to a JSON string for database storage.
    /// </summary>
    private static string ConvertTagsToJson(Dictionary<string, string> tags)
    {
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        return JsonSerializer.Serialize(tags, jsonOptions);
    }

    /// <summary>
    /// Converts a JSON string from the database back to a dictionary of tags.
    /// </summary>
    private static Dictionary<string, string> ConvertJsonToTags(string json)
    {
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, string>();

        var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json, jsonOptions);
        return result ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Compares two tag dictionaries to see if they are equal.
    /// </summary>
    private static bool AreTagsEqual(Dictionary<string, string>? tags1, Dictionary<string, string>? tags2)
    {
        if (ReferenceEquals(tags1, tags2))
            return true;

        if (tags1 is null || tags2 is null)
            return false;

        if (tags1.Count != tags2.Count)
            return false;

        return !tags1.Except(tags2).Any();
    }

    /// <summary>
    /// Generates a hash code for a tags dictionary.
    /// </summary>
    private static int GetTagsHashCode(Dictionary<string, string>? tags)
    {
        if (tags is null)
            return 0;

        var hash = 0;
        foreach (var (key, value) in tags)
        {
            var keyHash = key?.GetHashCode() ?? 0;
            var valueHash = value?.GetHashCode() ?? 0;
            hash = HashCode.Combine(hash, keyHash, valueHash);
        }
        return hash;
    }

    /// <summary>
    /// Creates a snapshot copy of the tags dictionary for change tracking.
    /// </summary>
    private static Dictionary<string, string> CreateTagsSnapshot(Dictionary<string, string>? tags)
    {
        return tags is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(tags);
    }
}
