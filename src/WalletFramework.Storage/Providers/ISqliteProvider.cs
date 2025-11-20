using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace WalletFramework.Storage.Providers;

public interface ISqliteProvider
{
    /// <summary>
    /// Initializes the SQLite provider.
    /// </summary>
    Unit Initialize();

    /// <summary>
    /// Configures the SQLite provider.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <param name="connectionString">The connection string.</param>
    Unit Configure(DbContextOptionsBuilder optionsBuilder, string connectionString);
}
