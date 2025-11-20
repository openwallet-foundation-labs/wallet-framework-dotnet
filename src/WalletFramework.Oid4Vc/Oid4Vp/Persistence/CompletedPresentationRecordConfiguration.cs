using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WalletFramework.Storage.Records;

namespace WalletFramework.Oid4Vc.Oid4Vp.Persistence;

public sealed record CompletedPresentationRecordConfiguration : IRecordConfiguration<CompletedPresentationRecord>
{
    public Unit Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<CompletedPresentationRecord>();
        entity.HasIndex(r => r.ClientId);
        entity.HasIndex(r => r.PresentationId).IsUnique();
        entity.Property(r => r.PresentationId).IsRequired();
        entity.Property(r => r.ClientId).IsRequired();
        entity.Property(r => r.Serialized).IsRequired();
        return Unit.Default;
    }
}
