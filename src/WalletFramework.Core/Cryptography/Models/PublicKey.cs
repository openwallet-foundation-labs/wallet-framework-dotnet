using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;
using static WalletFramework.Core.Base64Url.Base64UrlString;

namespace WalletFramework.Core.Cryptography.Models;

public record PublicKey(Base64UrlString X, Base64UrlString Y)
{
    public string KeyType => "EC";

    public string Curve => "P-256";

    public static Validation<PublicKey> ValidPublicKey(string x, string y)
    {
        var xCoordinate = ValidBase64UrlString(x);
        var yCoordinate = ValidBase64UrlString(y);

        return
            from xCoo in xCoordinate
            from yCoo in yCoordinate
            select new PublicKey(xCoo, yCoo);
    }
}
