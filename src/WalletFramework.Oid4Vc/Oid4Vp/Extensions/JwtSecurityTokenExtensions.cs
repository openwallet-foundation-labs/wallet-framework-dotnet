using System.IdentityModel.Tokens.Jwt;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using static Microsoft.IdentityModel.Tokens.Base64UrlEncoder;
using static Org.BouncyCastle.Security.SignerUtilities;
using static System.Text.Encoding;

namespace WalletFramework.Oid4Vc.Oid4Vp.Extensions
{
    /// <summary>
    ///     Extension methods for <see cref="JwtSecurityToken" />.
    /// </summary>
    public static class JwtSecurityTokenExtensions
    {
        /// <summary>
        ///     Validates the signature of the JWT token using the provided public key.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <param name="publicKey">The public key to use for validation.</param>
        /// <returns>True if the signature is valid, otherwise false.</returns>
        public static bool IsSignatureValid(this JwtSecurityToken token, ECPublicKeyParameters publicKey)
        {
            var signer = GetSigner("SHA-256withECDSA");

            signer.Init(false, publicKey);

            var encodedHeaderAndPayload = UTF8.GetBytes(token.EncodedHeader + "." + token.EncodedPayload);

            signer.BlockUpdate(encodedHeaderAndPayload, 0, encodedHeaderAndPayload.Length);

            return signer.VerifySignature(
                ConvertRawToDerFormat(
                    DecodeBytes(token.RawSignature)
                )
            );
        }

        private static byte[] ConvertRawToDerFormat(byte[] rawSignature)
        {
            if (rawSignature.Length != 64)
                throw new ArgumentException("Raw signature should be 64 bytes long", nameof(rawSignature));

            var r = new BigInteger(1, rawSignature.Take(32).ToArray());
            var s = new BigInteger(1, rawSignature.Skip(32).ToArray());

            var derSignature = new DerSequence(
                new DerInteger(r),
                new DerInteger(s)
            ).GetDerEncoded();

            return derSignature;
        }
    }
}
