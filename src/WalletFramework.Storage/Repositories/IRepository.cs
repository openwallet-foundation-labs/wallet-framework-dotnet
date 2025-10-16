using System.Linq.Expressions;
using LanguageExt;
using WalletFramework.Storage.Records;

namespace WalletFramework.Storage.Repositories;

/// <summary>
///     Generic repository abstraction for working with persisted records.
/// </summary>
/// <typeparam name="TRecord">A record type derived from <see cref="RecordBase" />.</typeparam>
public interface IRepository<TRecord> where TRecord : RecordBase
{
    /// <summary>
    ///     Adds a new record.
    /// </summary>
    /// <param name="record">The record to add.</param>
    Task<Unit> Add(TRecord record);

    /// <summary>
    ///     Adds multiple records.
    /// </summary>
    /// <param name="records">The records to add.</param>
    Task<Unit> AddMany(IEnumerable<TRecord> records);

    /// <summary>
    ///     Returns records matching the given predicate.
    /// </summary>
    /// <param name="predicate">Filter expression to apply.</param>
    Task<Option<IReadOnlyList<TRecord>>> Find(Expression<Func<TRecord, bool>> predicate);

    /// <summary>
    ///     Retrieves a record by identifier.
    /// </summary>
    /// <param name="id">The record identifier.</param>
    /// <returns>An option of the record if found.</returns>
    Task<Option<TRecord>> GetById(Guid id);

    /// <summary>
    ///     Returns all records.
    /// </summary>
    Task<Option<IReadOnlyList<TRecord>>> ListAll();

    /// <summary>
    ///     Removes a record.
    /// </summary>
    /// <param name="record">The record to remove.</param>
    Task<Unit> Remove(TRecord record);

    /// <summary>
    ///     Removes a record by identifier.
    /// </summary>
    /// <param name="id">The record identifier.</param>
    Task<Unit> RemoveById(Guid id);

    /// <summary>
    ///     Updates an existing record.
    /// </summary>
    /// <param name="record">The record to update.</param>
    Task<Unit> Update(TRecord record);
}
