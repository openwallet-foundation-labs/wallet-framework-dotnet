using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace WalletFramework.Oid4Vc.Oid4Vp.Jwk;

public static class JwkFun
{
    public static ECDiffieHellman ToEcdh(this JsonWebKey jwk)
    {
        var ecParameters = new ECParameters
        {
            Q =
            {
                X = Base64UrlEncoder.DecodeBytes(jwk.X),
                Y = Base64UrlEncoder.DecodeBytes(jwk.Y)
            },
            // TODO: Map curve from jwk
            Curve = ECCurve.NamedCurves.nistP256
        };

        return ECDiffieHellman.Create(ecParameters);
    }
}
