using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

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

    /// <summary>
    /// Computes the JWK SHA-256 Thumbprint as defined in RFC7638.
    /// </summary>
    /// <param name="jwk">The JsonWebKey to compute the thumbprint for.</param>
    /// <returns>The SHA-256 thumbprint as a byte array.</returns>
    public static byte[] GetThumbprint(JsonWebKey jwk)
    {
        // Create a JSON object with only the required members in lexicographic order
        var thumbprintJson = new JObject();
        
        switch (jwk.Kty?.ToUpper())
        {
            case "EC":
                // For Elliptic Curve keys: crv, kty, x, y
                if (string.IsNullOrEmpty(jwk.Crv)) throw new ArgumentException("EC key missing 'crv' parameter");
                if (string.IsNullOrEmpty(jwk.X)) throw new ArgumentException("EC key missing 'x' parameter");
                if (string.IsNullOrEmpty(jwk.Y)) throw new ArgumentException("EC key missing 'y' parameter");
                
                thumbprintJson["crv"] = jwk.Crv;
                thumbprintJson["kty"] = jwk.Kty;
                thumbprintJson["x"] = jwk.X;
                thumbprintJson["y"] = jwk.Y;
                break;
                
            case "RSA":
                // For RSA keys: e, kty, n
                if (string.IsNullOrEmpty(jwk.E)) throw new ArgumentException("RSA key missing 'e' parameter");
                if (string.IsNullOrEmpty(jwk.N)) throw new ArgumentException("RSA key missing 'n' parameter");
                
                thumbprintJson["e"] = jwk.E;
                thumbprintJson["kty"] = jwk.Kty;
                thumbprintJson["n"] = jwk.N;
                break;
                
            default:
                throw new ArgumentException($"Unsupported key type: {jwk.Kty}");
        }
        
        // Convert to compact JSON (no whitespace)
        var jsonString = thumbprintJson.ToString(Newtonsoft.Json.Formatting.None);
        
        // Compute SHA-256 hash
        var jsonBytes = Encoding.UTF8.GetBytes(jsonString);
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(jsonBytes);
        
        // Return raw hash bytes
        return hashBytes;
    }
}
