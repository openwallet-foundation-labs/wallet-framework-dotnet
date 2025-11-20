namespace WalletFramework.Storage.Records;

/// <summary>
/// Builder interface for configuring wallet storage with records.
/// </summary>
public interface IRecordsBuilder
{
    /// <summary>
    /// Adds a record configuration to the storage builder.
    /// </summary>
    /// <typeparam name="TRecord">The record type that implements IRecord and inherits from BaseRecord.</typeparam>
    /// <typeparam name="TConfiguration">The configuration type that implements IRecordConfiguration&lt;TRecord&gt;.</typeparam>
    /// <returns>The storage builder for chaining.</returns>
    IRecordsBuilder AddRecord<TRecord, TConfiguration>()
        where TRecord : RecordBase
        where TConfiguration : class, IRecordConfiguration<TRecord>;

    /// <summary>
    /// Adds a record configuration instance to the storage builder.
    /// </summary>
    /// <typeparam name="TRecord">The record type that implements IRecord and inherits from BaseRecord.</typeparam>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The storage builder for chaining.</returns>
    IRecordsBuilder AddRecord<TRecord>(IRecordConfiguration<TRecord> configuration)
        where TRecord : RecordBase;
}
