using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WalletFramework.Storage.Records;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Persistence;

public sealed record AuthFlowSessionRecordConfiguration : IRecordConfiguration<AuthFlowSessionRecord>
{
    public Unit Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AuthFlowSessionRecord>();
        entity.HasIndex(r => r.SessionState).IsUnique();
        entity.Property(r => r.SessionState).IsRequired();
        entity.Property(r => r.Serialized).IsRequired();
        return Unit.Default;
    }
}
