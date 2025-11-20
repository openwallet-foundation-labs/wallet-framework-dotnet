using LanguageExt;
using WalletFramework.Storage.Records;

namespace WalletFramework.Storage;

public interface IDomainRepository<TDomainModel, TRecord, in TId> where TRecord : RecordBase
{
    Task<Unit> Add(TDomainModel domainModel);

    Task<Unit> AddMany(IEnumerable<TDomainModel> domainModels);

    Task<Option<TDomainModel>> GetById(TId id);

    Task<Option<List<TDomainModel>>> Find(ISearchConfig<TRecord> config);

    Task<Option<List<TDomainModel>>> ListAll();

    Task<Unit> Update(TDomainModel domainModel);

    Task<Unit> Delete(TId id);
    
    Task<Unit> Delete(TDomainModel domainModel);
}
