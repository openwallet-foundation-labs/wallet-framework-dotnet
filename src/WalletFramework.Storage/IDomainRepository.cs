using LanguageExt;
using WalletFramework.Storage.Records;

namespace WalletFramework.Storage;

public interface IDomainRepository<TDomain, TRecord, in TId> where TRecord : RecordBase
{
    Task<Unit> Add(TDomain domain);

    Task<Unit> AddMany(IEnumerable<TDomain> domains);

    Task<Option<TDomain>> GetById(TId id);

    Task<Option<List<TDomain>>> Find(ISearchConfig<TRecord> config);

    Task<Option<List<TDomain>>> ListAll();

    Task<Unit> Update(TDomain domain);

    Task<Unit> Delete(TId id);
    
    Task<Unit> Delete(TDomain domain);
}
