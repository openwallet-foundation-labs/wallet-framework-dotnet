using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using WalletFramework.Core.Cryptography.Abstractions;
using WalletFramework.Core.Cryptography.Models;

namespace WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

public class SdJwtSignerService : ISdJwtSignerService
{
    private readonly IKeyStore _keyStore;

    public SdJwtSignerService(IKeyStore keyStore) => _keyStore = keyStore;

    public async Task<string> GenerateKbProofOfPossessionAsync(KeyId keyId, string audience, string nonce, string type, string? sdHash, string? clientId)
    {
        var header = new Dictionary<string, object>
        {
            { "alg", "ES256" },
            { "typ", type }
        };

        if (string.Equals(type, "openid4vci-proof+jwt", StringComparison.OrdinalIgnoreCase))
        {
            var jwkSerialized = await _keyStore.LoadKey(keyId);
            var jwkDeserialized = JsonConvert.DeserializeObject(jwkSerialized);
            if (jwkDeserialized != null)
            {
                header["jwk"] = jwkDeserialized;
            }
        }

        var payload = new
        {
            aud = audience,
            nonce,
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            sd_hash = sdHash,
            iss = clientId
        };

        return await CreateSignedJwt(header, payload, keyId);
    }
    
    public async Task<string> CreateSignedJwt(object header, object payload, KeyId keyId)
    {
        var serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        var encodedHeader = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(header, serializerSettings));
        var encodedPayload = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(payload, serializerSettings));

        var dataToSign = encodedHeader + "." + encodedPayload;
        var signedData = await _keyStore.Sign(keyId, Encoding.UTF8.GetBytes(dataToSign));
        var rawSignature = ConvertDerToRawFormat(signedData);

        var encodedSignature = Base64UrlEncoder.Encode(rawSignature);
        return encodedHeader + "." + encodedPayload + "." + encodedSignature;
    }
    
    private static byte[] ConvertDerToRawFormat(byte[]? derSignature)
    {
        var seq = (Asn1Sequence)Asn1Object.FromByteArray(derSignature);
        var r = ((DerInteger)seq[0]).Value;
        var s = ((DerInteger)seq[1]).Value;
        var rawSignature = new byte[64];
        var rBytes = r.ToByteArrayUnsigned();
        var sBytes = s.ToByteArrayUnsigned();
        Array.Copy(rBytes, Math.Max(0, rBytes.Length - 32), rawSignature, 0, Math.Min(32, rBytes.Length));
        Array.Copy(sBytes, Math.Max(0, sBytes.Length - 32), rawSignature, 32, Math.Min(32, sBytes.Length));
        return rawSignature;
    }
}
