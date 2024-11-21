using Hyperledger.Aries.Storage;
using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.MdocVc;
using WalletFramework.Oid4Vc.CredentialSet.Models;
using WalletFramework.SdJwtVc.Models.Records;

namespace WalletFramework.Oid4Vc.CredentialSet;

public interface ICredentialSetService
{
    Task DeleteAsync(CredentialSetId credentialSetId);

    Task<Option<CredentialSetRecord>> GetAsync(
        CredentialSetId credentialSetId);
    
    Task AddAsync(CredentialSetRecord credentialSetRecord);
    
    Task UpdateAsync(CredentialSetRecord credentialSetRecord);
    
    Task<Option<IEnumerable<CredentialSetRecord>>> ListAsync(
        Option<ISearchQuery> query, 
        int count = 100, 
        int skip = 0);
    
    Task<Option<IEnumerable<SdJwtRecord>>> GetAssociatedSdJwtRecords(CredentialSetId credentialSetId);
    
    Task<Option<IEnumerable<MdocRecord>>> GetAssociatedMDocRecords(CredentialSetId credentialSetId);
    
    Task<CredentialSetRecord> RefreshCredentialSetState(CredentialSetRecord credentialSetRecord);
    
    Task RefreshCredentialSetStates();
}
