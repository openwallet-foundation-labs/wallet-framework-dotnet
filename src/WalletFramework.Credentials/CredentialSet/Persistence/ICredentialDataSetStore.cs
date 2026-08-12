using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.Credentials.CredentialSet.Models;

namespace WalletFramework.Credentials.CredentialSet.Persistence;

public interface ICredentialDataSetStore
{
    Task<Unit> AddMany(IEnumerable<CredentialDataSet> credentialDataSets);

    Task<Unit> Delete(CredentialSetId id);

    Task<Option<CredentialDataSet>> Get(CredentialSetId id);

    Task<IReadOnlyList<CredentialDataSet>> List();
    
    Task<Unit> Save(CredentialDataSet credentialDataSet);
}
