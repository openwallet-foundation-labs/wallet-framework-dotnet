using Hyperledger.Aries.Extensions;
using Jose;
using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Jwk;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption;

public record EncryptedAuthorizationResponse(string Jwe)
{
    public override string ToString() => Jwe;
}

public static class EncryptedAuthorizationResponseFun
{
    public static FormUrlEncodedContent ToFormUrl(this EncryptedAuthorizationResponse response)
    {
        var content = new Dictionary<string, string>
        {
            { "response", response.ToString() },
        };

        return new FormUrlEncodedContent(content);
    }

    public static EncryptedAuthorizationResponse Encrypt(
        this AuthorizationResponse response,
        AuthorizationRequest authorizationRequest,
        Option<Nonce> mdocNonce)
    {
        var verifierPubKey = authorizationRequest.ClientMetadata!.Jwks.First();
        
        var headers = new Dictionary<string, object>
        {
            { "apv", authorizationRequest.Nonce },
            { "kid", verifierPubKey.Kid }
        };

        mdocNonce.IfSome(nonce => headers.Add("apu", nonce.AsBase64Url.ToString()));
        
        var jwe = JWE.EncryptBytes(
            response.ToJson().GetUTF8Bytes(),
            new [] { new JweRecipient(JweAlgorithm.ECDH_ES, verifierPubKey.ToEcdh()) },
            JweEncryption.A128CBC_HS256,
            mode: SerializationMode.Compact,
            extraProtectedHeaders: headers);

        return new EncryptedAuthorizationResponse(jwe);
    }
}
