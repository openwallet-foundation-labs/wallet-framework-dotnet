using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.Oid4Vc.CredentialSet.Models;

namespace WalletFramework.Oid4Vc.CredentialSet.Persistence;

public interface ICredentialDataSetStore
{
    Task<Unit> AddMany(IEnumerable<CredentialDataSet> credentialDataSets);

    Task<Unit> Delete(CredentialSetId id);

    Task<Option<CredentialDataSet>> Get(CredentialSetId id);

    Task<IReadOnlyList<CredentialDataSet>> List();
    
    Task<Unit> Save(CredentialDataSet credentialDataSet);
}
