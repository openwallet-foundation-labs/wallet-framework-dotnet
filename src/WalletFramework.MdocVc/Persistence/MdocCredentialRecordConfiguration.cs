using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WalletFramework.Storage.Records;

namespace WalletFramework.MdocVc.Persistence;

public sealed record MdocCredentialRecordConfiguration : IRecordConfiguration<MdocCredentialRecord>
{
    public Unit Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<MdocCredentialRecord>();
        entity.HasIndex(r => r.DocType);

        return Unit.Default;
    }
}

