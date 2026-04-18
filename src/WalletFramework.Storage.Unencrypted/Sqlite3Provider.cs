using LanguageExt;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using WalletFramework.Storage.Providers;

namespace WalletFramework.Storage.Unencrypted;

/// <summary>
/// WARNING: This provider stores data without encryption. Do not use for sensitive data.
/// </summary>
public sealed class Sqlite3Provider : ISqliteProvider
{
    public Unit Initialize()
    {
        Batteries_V2.Init();
        return Unit.Default;
    }

    public Unit Configure(DbContextOptionsBuilder optionsBuilder, string connectionString)
    {
        optionsBuilder.UseSqlite(connectionString);
        return Unit.Default;
    }
}
