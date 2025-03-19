using System.Text;
using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using WalletFramework.Core.Cryptography.Abstractions;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.String;

namespace WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

public class SdJwtSigner(IKeyStore keyStore) : ISdJwtSigner
{
    public async Task<string> GenerateKbProofOfPossessionAsync(
        KeyId keyId,
        string audience,
        string nonce,
        string type,
        string? sdHash,
        string? clientId,
        Option<IEnumerable<string>> transactionDataBase64UrlStrings,
        Option<IEnumerable<string>> transactionDataHashes,
        Option<string> transactionDataHashesAlg)
    {
        var header = new Dictionary<string, object>
        {
            { "alg", "ES256" },
            { "typ", type }
        };

        if (string.Equals(type, "openid4vci-proof+jwt", StringComparison.OrdinalIgnoreCase))
        {
            var publicKey = await keyStore.GetPublicKey(keyId);
            header["jwk"] = publicKey.ToObj();
        }

        var payload = new Dictionary<string, object>
        {
            { "aud", audience },
            { "nonce", nonce },
            { "iat" , DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };

        transactionDataBase64UrlStrings.IfSome(base64UrlStrings => 
            payload.Add("transaction_data", base64UrlStrings.ToArray())
        );

        transactionDataHashes.IfSome(hashes => 
            payload.Add("transaction_data_hashes", hashes.ToArray())
        );

        transactionDataHashesAlg.IfSome(hashesAlg =>
            payload.Add("transaction_data_hashes_alg", hashesAlg)
        );

        if (!sdHash.IsNullOrEmpty())
            payload["sd_hash"] = sdHash!;
        
        if (!clientId.IsNullOrEmpty())
            payload["iss"] = clientId!;

        return await CreateSignedJwt(header, payload, keyId);
    }
    
    public async Task<string> CreateSignedJwt(object header, object payload, KeyId keyId)
    {
        var encodedHeader = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(header));
        var encodedPayload = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(payload));

        var dataToSign = encodedHeader + "." + encodedPayload;
        var signature = await keyStore.Sign(keyId, Encoding.UTF8.GetBytes(dataToSign));

        var encodedSignature = Base64UrlEncoder.Encode(signature);
        return encodedHeader + "." + encodedPayload + "." + encodedSignature;
    }
}
