using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace WalletFramework.Storage.Records;

/// <summary>
///     Non-generic base interface for record configurations.
/// </summary>
public interface IRecordConfiguration
{
    /// <summary>
    ///     Configures the Entity Framework model for the record type.
    /// </summary>
    /// <param name="modelBuilder">The model builder to configure.</param>
    Unit Configure(ModelBuilder modelBuilder);
}

/// <summary>
///     Interface for configuring Entity Framework for a specific record type.
/// </summary>
/// <typeparam name="TRecord">The record type that implements IRecord and inherits from BaseRecord.</typeparam>
public interface IRecordConfiguration<TRecord> : IRecordConfiguration where TRecord : RecordBase;
