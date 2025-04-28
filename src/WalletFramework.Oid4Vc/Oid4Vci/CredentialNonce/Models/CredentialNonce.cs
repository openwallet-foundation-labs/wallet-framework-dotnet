using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Models;

public readonly struct CredentialNonce
{
    public string Value { get; }
    
    private CredentialNonce(string nonce)
    {
        Value = nonce;
    }
    
    public static Validation<CredentialNonce> ValidCredentialNonce(string nonce)
    {
        if (string.IsNullOrEmpty(nonce))
            return new StringIsNullOrWhitespaceError<CredentialNonce>();

        return new CredentialNonce(nonce);
    }
};
