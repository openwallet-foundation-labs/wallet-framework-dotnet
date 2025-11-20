using WalletFramework.Oid4Vc.CredentialSet.Models;

namespace WalletFramework.Oid4Vc.CredentialSet;

public interface ICredentialSetService
{
    Task<CredentialDataSet> RefreshCredentialSetState(CredentialDataSet credentialDataSet);
    
    Task RefreshCredentialSetStates();
}
