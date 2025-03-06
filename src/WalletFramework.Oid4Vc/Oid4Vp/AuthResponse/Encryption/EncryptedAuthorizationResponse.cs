using Hyperledger.Aries.Extensions;
using Jose;
using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.Jwk;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption;

public record EncryptedAuthorizationResponse(string Jwe, Option<string> State)
{
    public override string ToString() => Jwe;
}

public static class EncryptedAuthorizationResponseFun
{
    public static EncryptedAuthorizationResponse Encrypt(
        this AuthorizationResponse response,
        AuthorizationRequest authorizationRequest,
        Option<Nonce> mdocNonce) => Encrypt(
        response,
        authorizationRequest.ClientMetadata!.Jwks.First(),
        authorizationRequest.Nonce,
        authorizationRequest.ClientMetadata.AuthorizationEncryptedResponseEnc,
        mdocNonce);

    private static EncryptedAuthorizationResponse Encrypt(
        this AuthorizationResponse response,
        JsonWebKey verifierPubKey,
        string apv,
        Option<string> authorizationEncryptedResponseEnc,
        Option<Nonce> mdocNonce)
    {
        var apvBase64 = Base64UrlString.CreateBase64UrlString(apv.GetUTF8Bytes());

        var headers = new Dictionary<string, object>
        {
            { "apv", apvBase64.ToString() },
            { "kid", verifierPubKey.Kid }
        };

        mdocNonce.IfSome(nonce => headers.Add("apu", nonce.AsBase64Url.ToString()));

        var settings = new JwtSettings();
        settings.RegisterJwe(JweEncryption.A256GCM, new AesGcmEncryption());

        var jwe = JWE.EncryptBytes(
            response.ToJson().GetUTF8Bytes(),
            new[] { new JweRecipient(JweAlgorithm.ECDH_ES, verifierPubKey.ToEcdh()) },
            authorizationEncryptedResponseEnc.ToNullable() switch {
                "A256GCM" => JweEncryption.A256GCM,
                "A128CBC-HS256" => JweEncryption.A128CBC_HS256,
                null => JweEncryption.A256GCM,
                _ => throw new NotSupportedException("Unsupported response encryption algorithm requested by verifier.")
            },
            mode: SerializationMode.Compact,
            extraProtectedHeaders: headers,
            settings: settings);

        return new EncryptedAuthorizationResponse(jwe, response.State);
    }

    public static FormUrlEncodedContent ToFormUrl(this EncryptedAuthorizationResponse response)
    {
        var content = new Dictionary<string, string>
        {
            { "response", response.ToString() }
        };

        response.State.IfSome(state => content["state"] = state);

        return new FormUrlEncodedContent(content);
    }

    private class AesGcmEncryption : IJweAlgorithm
    {
        public int KeySize => 256;

        public byte[] Decrypt(byte[] aad, byte[] cek, byte[] iv, byte[] cipherText, byte[] authTag)
        {
            var gcmBlockCipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(cek), 128, iv, aad);
            gcmBlockCipher.Init(false, parameters);

            var combinedCipherText = new byte[cipherText.Length + authTag.Length];
            Array.Copy(cipherText, 0, combinedCipherText, 0, cipherText.Length);
            Array.Copy(authTag, 0, combinedCipherText, cipherText.Length, authTag.Length);

            var plainText = new byte[gcmBlockCipher.GetOutputSize(combinedCipherText.Length)];
            var len = gcmBlockCipher.ProcessBytes(combinedCipherText, 0, combinedCipherText.Length, plainText, 0);
            gcmBlockCipher.DoFinal(plainText, len);

            return plainText;
        }
        
        public byte[][] Encrypt(byte[] aad, byte[] plainText, byte[] cek)
        {
            var iv = new byte[12];
            new SecureRandom().NextBytes(iv);

            var gcmBlockCipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(cek), 128, iv, aad);
            gcmBlockCipher.Init(true, parameters);

            var cipherText = new byte[gcmBlockCipher.GetOutputSize(plainText.Length)];
            var len = gcmBlockCipher.ProcessBytes(plainText, 0, plainText.Length, cipherText, 0);
            gcmBlockCipher.DoFinal(cipherText, len);

            var authTag = new byte[16];
            Array.Copy(cipherText, cipherText.Length - authTag.Length, authTag, 0, authTag.Length);

            var actualCipherText = new byte[cipherText.Length - authTag.Length];
            Array.Copy(cipherText, 0, actualCipherText, 0, actualCipherText.Length);

            return [iv, actualCipherText, authTag];
        }
    }
}
