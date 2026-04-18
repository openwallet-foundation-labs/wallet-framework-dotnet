using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WalletFramework.Storage.Records;

namespace WalletFramework.SdJwtVc.Persistence;

public sealed record SdJwtCredentialRecordConfiguration : IRecordConfiguration<SdJwtCredentialRecord>
{
    public Unit Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<SdJwtCredentialRecord>();
        entity.HasIndex(r => r.Vct);
        entity.HasIndex(r => r.CredentialSetId);

        return Unit.Default;
    }
}
