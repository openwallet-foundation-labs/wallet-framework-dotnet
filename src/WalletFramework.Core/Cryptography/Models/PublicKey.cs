using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;

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

    public static PublicKey ToPubKey(this ECPrivateKeyParameters parameters)
    {
        var x = parameters.Parameters.G.AffineXCoord.ToBigInteger().ToByteArrayUnsigned();
        var y = parameters.Parameters.G.AffineYCoord.ToBigInteger().ToByteArrayUnsigned();
        
        // TODO: Also persist D
        
        return new PublicKey(
            Base64UrlString.CreateBase64UrlString(x),
            Base64UrlString.CreateBase64UrlString(y));
    }
    
    public static PublicKey ToPubKey(this ECPublicKeyParameters parameters)
    {
        var x = parameters.Q.AffineXCoord.ToBigInteger().ToByteArrayUnsigned();
        var y = parameters.Q.AffineYCoord.ToBigInteger().ToByteArrayUnsigned();
        
        return new PublicKey(
            Base64UrlString.CreateBase64UrlString(x),
            Base64UrlString.CreateBase64UrlString(y));
    }
    
    public static ECPublicKeyParameters ToECPublicKeyParameters(this PublicKey publicKey)
    {
        var curve = NistNamedCurves.GetByName(publicKey.Curve);
        var domainParameters = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);

        var x64 = Base64UrlString.FromString(publicKey.X.AsString).UnwrapOrThrow().AsByteArray;
        var y64 = Base64UrlString.FromString(publicKey.Y.AsString).UnwrapOrThrow().AsByteArray;
        
        var x = new BigInteger(1, x64);
        var y = new BigInteger(1, y64);
        var q = curve.Curve.CreatePoint(x, y);

        return new ECPublicKeyParameters(q, domainParameters);
    }
}
