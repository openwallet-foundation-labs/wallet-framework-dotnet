using System.IdentityModel.Tokens.Jwt;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using static Microsoft.IdentityModel.Tokens.Base64UrlEncoder;
using static Org.BouncyCastle.Security.SignerUtilities;
using static System.Text.Encoding;

namespace WalletFramework.Oid4Vc.Oid4Vp.Extensions;

/// <summary>
///     Extension methods for <see cref="JwtSecurityToken" />.
/// </summary>
public static class JwtSecurityTokenExtensions
{
    /// <summary>
    ///     Validates the signature of the JWT token using the provided public key.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <param name="publicKeyParameters">The public key to use for validation.</param>
    /// <returns>True if the signature is valid, otherwise false.</returns>
    public static bool IsSignatureValid(this JwtSecurityToken token, AsymmetricKeyParameter publicKeyParameters)
    {
        var encodedHeaderAndPayload = UTF8.GetBytes(token.EncodedHeader + "." + token.EncodedPayload);
        return token.Header.Alg switch
        {
            "RS256" => GetSigner("SHA-256withRSA").SignWithRaw(encodedHeaderAndPayload, publicKeyParameters, token.RawSignature),
            "ES256" => GetSigner("SHA-256withECDSA").SignWithDer(encodedHeaderAndPayload, publicKeyParameters, token.RawSignature),
            "ES384" => GetSigner("SHA-384withECDSA").SignWithDer(encodedHeaderAndPayload, publicKeyParameters, token.RawSignature),
            "ES512" => GetSigner("SHA-512withECDSA").SignWithDer(encodedHeaderAndPayload, publicKeyParameters, token.RawSignature),
            _ => throw new InvalidOperationException("Unsupported JWT alg")
        };
    }

    private static byte[] ConvertRawToDerFormat(byte[] rawSignature)
    {
        var bytesLength = rawSignature.Length / 2;

        var r = new BigInteger(1, rawSignature.Take(bytesLength).ToArray());
        var s = new BigInteger(1, rawSignature.Skip(bytesLength).ToArray());

        var derSignature = new DerSequence(new DerInteger(r), new DerInteger(s));

        return derSignature.GetDerEncoded();
    }

    private static bool SignWithDer(
        this ISigner signer,
        IEnumerable<byte> bytes,
        AsymmetricKeyParameter publicKeyParameters,
        string rawSignature)
    {
        var bytesArray = bytes.ToArray();
        var rawBytes = DecodeBytes(rawSignature);
        var derBytes = ConvertRawToDerFormat(rawBytes);

        signer.Init(false, publicKeyParameters);
        signer.BlockUpdate(bytesArray, 0, bytesArray.Length);
        return signer.VerifySignature(derBytes);
    }
    
    private static bool SignWithRaw(
        this ISigner signer,
        IEnumerable<byte> bytes,
        AsymmetricKeyParameter publicKeyParameters,
        string rawSignature)
    {
        var bytesArray = bytes.ToArray();
        var rawBytes = DecodeBytes(rawSignature);

        signer.Init(false, publicKeyParameters);
        signer.BlockUpdate(bytesArray, 0, bytesArray.Length);
        return signer.VerifySignature(rawBytes);
    }
}
