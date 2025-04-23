using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Abstractions;

public interface ICNonceService
{
    Task<CNonce> GetCredentialNonce(CNonceEndpoint cNonceEndpoint);
}
