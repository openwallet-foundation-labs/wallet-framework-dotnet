using LanguageExt;
using WalletFramework.MdocVc;
using WalletFramework.Oid4Vc.CredentialSet.Models;
using WalletFramework.SdJwtVc.Models.Records;

namespace WalletFramework.Oid4Vc.CredentialSet;

public interface ICredentialSetService
{
    Task DeleteAsync(string credentialSetId);
    
    Task AddAsync(CredentialSetRecord credentialSetRecord);
    
    Task UpdateAsync(CredentialSetRecord credentialSetRecord);
    
    Task<Option<IEnumerable<SdJwtRecord>>> GetAssociatedSdJwtRecords(string credentialSetId);
    
    Task<Option<IEnumerable<MdocRecord>>> GetAssociatedMDocRecords(string credentialSetId);
}
