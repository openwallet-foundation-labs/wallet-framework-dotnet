using WalletFramework.Credentials.CredentialSet.Models;

namespace WalletFramework.Credentials.CredentialSet;

public interface ICredentialSetService
{
    Task<CredentialDataSet> RefreshCredentialSetState(CredentialDataSet credentialDataSet);
    
    Task RefreshCredentialSetStates();
}
