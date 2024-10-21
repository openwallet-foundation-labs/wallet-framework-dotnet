namespace WalletFramework.Core.Credentials.Abstractions;

/// <summary>
///     This interface is used to represent a credential.
/// </summary>
public interface ICredential
{
    CredentialId GetId();
    
    string GetCredentialSetId();
}
