using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WalletFramework.Storage.Records;

namespace WalletFramework.Storage.Tests.TestModels;

/// <summary>
///     Entity Framework configuration for TestRecord.
/// </summary>
public record TestRecordConfiguration : IRecordConfiguration<TestRecord>
{
    public Unit Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TestRecord>();

        entity.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(500);

        entity.Property(r => r.Value)
            .IsRequired();

        entity.Property(r => r.IsActive)
            .IsRequired();

        return Unit.Default;
    }
}
