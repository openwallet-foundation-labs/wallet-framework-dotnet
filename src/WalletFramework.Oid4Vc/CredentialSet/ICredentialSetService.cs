using WalletFramework.Oid4Vc.CredentialSet.Models;

namespace WalletFramework.Oid4Vc.CredentialSet;

public interface ICredentialSetService
{
    Task<CredentialSetRecord> RefreshCredentialSetState(CredentialSetRecord credentialSetRecord);
    
    Task RefreshCredentialSetStates();
}
