using Microsoft.EntityFrameworkCore;
using WalletFramework.Storage.Records;

namespace WalletFramework.Storage.Database;

public class WalletDbContext(
    DbContextOptions<WalletDbContext> options,
    IEnumerable<IRecordConfiguration> configurations) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (var configuration in configurations)
        {
            configuration.Configure(modelBuilder);
        }
    }
}
