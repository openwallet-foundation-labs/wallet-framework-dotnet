using WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Abstractions;

public interface ICredentialNonceService
{
    Task<Models.CredentialNonce> GetCredentialNonce(CredentialNonceEndpoint credentialNonceEndpoint);
}
