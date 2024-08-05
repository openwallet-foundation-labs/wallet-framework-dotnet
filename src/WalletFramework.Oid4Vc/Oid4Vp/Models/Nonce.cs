using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public readonly struct Nonce
{
    public Base64UrlString Value { get; }

    private Nonce(Base64UrlString value)
    {
        Value = value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Nonce nonce) => nonce.Value;
    
    public static Validation<Nonce> ValidNonce(string nonce) =>
        from base64UrlString in Base64UrlString.ValidBase64UrlString(nonce)
        select new Nonce(base64UrlString);

    public static Nonce GenerateNonce()
    {
        const int nonceLength = 16;
        var nonceBytes = new byte[nonceLength];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(nonceBytes);
        }
        
        var encoded = Base64UrlEncoder.Encode(nonceBytes);
        var base64Url = Base64UrlString.ValidBase64UrlString(encoded).UnwrapOrThrow();
        
        return new Nonce(base64Url); 
    }
}

public static class NonceFun
{
    public static byte[] ToByteString(this Nonce nonce) => nonce.Value.ToByteString();
}
