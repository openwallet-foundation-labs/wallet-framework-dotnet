using Hyperledger.Aries.Storage;
using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.Oid4Vc.CredentialSet.Models;

namespace WalletFramework.Oid4Vc.CredentialSet;

public interface ICredentialSetStorage
{
    Task Delete(CredentialSetId credentialSetId);

    Task<Option<CredentialSetRecord>> Get(
        CredentialSetId credentialSetId);
    
    Task Add(CredentialSetRecord credentialSetRecord);
    
    Task Update(CredentialSetRecord credentialSetRecord);
    
    Task<Option<IEnumerable<CredentialSetRecord>>> List(
        Option<ISearchQuery> query, 
        int count = 100, 
        int skip = 0);
}
