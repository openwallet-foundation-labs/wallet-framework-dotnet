using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Models;

public readonly struct CNonce
{
    public string Value { get; }
    
    private CNonce(string nonce)
    {
        Value = nonce;
    }
    
    public static Validation<CNonce> ValidCredentialNonce(string nonce)
    {
        if (string.IsNullOrEmpty(nonce))
            return new StringIsNullOrWhitespaceError<CNonce>();

        return new CNonce(nonce);
    }
};
