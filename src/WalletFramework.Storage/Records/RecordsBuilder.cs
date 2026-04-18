using Microsoft.Extensions.DependencyInjection;

namespace WalletFramework.Storage.Records;

/// <summary>
///     Builder class for configuring wallet storage with records.
/// </summary>
public sealed class RecordsBuilder(IServiceCollection services) : IRecordsBuilder
{
    /// <inheritdoc />
    public IRecordsBuilder AddRecord<TRecord, TConfiguration>()
        where TRecord : RecordBase
        where TConfiguration : class, IRecordConfiguration<TRecord>
    {
        services.AddScoped<IRecordConfiguration<TRecord>, TConfiguration>();
        services.AddScoped<IRecordConfiguration>(sp => sp.GetRequiredService<IRecordConfiguration<TRecord>>());
        return this;
    }

    /// <inheritdoc />
    public IRecordsBuilder AddRecord<TRecord>(IRecordConfiguration<TRecord> configuration)
        where TRecord : RecordBase
    {
        services.AddScoped(_ => configuration);
        services.AddScoped<IRecordConfiguration>(sp => sp.GetRequiredService<IRecordConfiguration<TRecord>>());
        return this;
    }
}
