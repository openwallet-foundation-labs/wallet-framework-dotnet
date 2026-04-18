using System.Linq.Expressions;
using WalletFramework.Storage.Records;

namespace WalletFramework.Storage;

/// <summary>
///     A search configuration for a record type.
/// </summary>
/// <typeparam name="TRecord">The record type to search.</typeparam>
public interface ISearchConfig<TRecord> where TRecord : RecordBase
{
    /// <summary>
    ///     Converts the search configuration to a predicate.
    /// </summary>
    Expression<Func<TRecord, bool>> ToPredicate();
}
