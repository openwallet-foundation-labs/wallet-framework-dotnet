using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace WalletFramework.Oid4Vp.Tests.AuthRequest.Samples;

public static class UnresolvedClientIdSchemeSamples
{
    private const string ResponseUri = "https://verifier.example.com/response";

    public static string RequestObjectWithoutClientId()
    {
        var payload = new JwtPayload
        {
            { "response_uri", ResponseUri },
            { "nonce", "test-nonce-value" },
            { "response_mode", "direct_post.jwt" }
        };

        return BuildUnsignedRequestObject(payload);
    }

    public static string RequestObjectWithUnsupportedClientIdPrefix()
    {
        var payload = new JwtPayload
        {
            { "response_uri", ResponseUri },
            { "client_id", "unknown_scheme:verifier.example.com" },
            { "nonce", "test-nonce-value" },
            { "response_mode", "direct_post.jwt" }
        };

        return BuildUnsignedRequestObject(payload);
    }

    private static string BuildUnsignedRequestObject(JwtPayload payload)
    {
        var encodedHeader = Base64UrlEncoder.Encode("{\"alg\":\"none\",\"typ\":\"JWT\"}");
        var encodedPayload = Base64UrlEncoder.Encode(payload.SerializeToJson());

        return $"{encodedHeader}.{encodedPayload}";
    }
}
