using WalletFramework.Core.Base64Url;

namespace WalletFramework.Core.Cryptography.Models;

public record PublicKey(Base64UrlString X, Base64UrlString Y)
{
    public string KeyType => "EC";

    public string Curve => "P-256";
}

public static class PublicKeyFun
{
    public static object ToObj(this PublicKey publicKey) => new
    {
        kty = publicKey.KeyType,
        crv = publicKey.Curve,
        x = publicKey.X.ToString(),
        y = publicKey.Y.ToString()
    };
}
