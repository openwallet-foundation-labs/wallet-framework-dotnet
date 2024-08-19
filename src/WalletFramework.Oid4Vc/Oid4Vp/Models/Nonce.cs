using System.Security.Cryptography;
using WalletFramework.Core.Base64Url;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public readonly struct Nonce
{
    private byte[] Value { get; }

    private Nonce(byte[] value)
    {
        Value = value;
    }

    public Base64UrlString AsBase64Url => Base64UrlString.CreateBase64UrlString(Value);

    public static Nonce GenerateNonce()
    {
        const int nonceLength = 16;
        var nonceBytes = new byte[nonceLength];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(nonceBytes);
        }
        
        return new Nonce(nonceBytes); 
    }
}
