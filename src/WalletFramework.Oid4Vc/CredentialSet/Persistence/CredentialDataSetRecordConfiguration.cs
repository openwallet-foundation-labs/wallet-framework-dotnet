using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WalletFramework.Storage.Records;

namespace WalletFramework.Oid4Vc.CredentialSet.Persistence;

public sealed record CredentialDataSetRecordConfiguration : IRecordConfiguration<CredentialDataSetRecord>
{
    public Unit Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<CredentialDataSetRecord>();
        entity.HasIndex(r => r.IssuerId);
        entity.HasIndex(r => r.SdJwtCredentialType);
        entity.HasIndex(r => r.MDocCredentialType);
        return Unit.Default;
    }
}
