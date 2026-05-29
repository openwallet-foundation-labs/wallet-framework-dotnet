using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace WalletFramework.Oid4Vp.Tests.AuthRequest.Samples;

public static class X509HashSamples
{
    private const string ResponseUri = "https://verifier.example.com/response";

    public static string SignedRequestObjectWithMatchingCertificateHash()
    {
        var chain = CreateLeafSignedByCa();
        var clientId = $"x509_hash:{LeafCertificateHash(chain.Leaf)}";

        return BuildSignedRequestObject(clientId, chain, includeX5c: true);
    }

    public static string SignedRequestObjectWithMismatchedCertificateHash()
    {
        var chain = CreateLeafSignedByCa();
        const string clientId = "x509_hash:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

        return BuildSignedRequestObject(clientId, chain, includeX5c: true);
    }

    public static string SignedRequestObjectWithoutX5c()
    {
        var chain = CreateLeafSignedByCa();
        const string clientId = "x509_hash:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

        return BuildSignedRequestObject(clientId, chain, includeX5c: false);
    }

    private static string LeafCertificateHash(X509Certificate2 leaf)
    {
        var hash = SHA256.HashData(leaf.RawData);
        return Base64UrlEncoder.Encode(hash);
    }

    private static string BuildSignedRequestObject(string clientId, CertificateChain chain, bool includeX5c)
    {
        var signingCredentials = new SigningCredentials(
            new RsaSecurityKey(chain.LeafKey),
            SecurityAlgorithms.RsaSha256);

        var header = new JwtHeader(signingCredentials);
        if (includeX5c)
        {
            header["x5c"] = new[]
            {
                Convert.ToBase64String(chain.Leaf.RawData),
                Convert.ToBase64String(chain.Ca.RawData)
            };
        }

        var payload = new JwtPayload
        {
            { "response_uri", ResponseUri },
            { "client_id", clientId },
            { "nonce", "test-nonce-value" },
            { "response_mode", "direct_post.jwt" }
        };

        var token = new JwtSecurityToken(header, payload);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static CertificateChain CreateLeafSignedByCa()
    {
        var caKey = RSA.Create(2048);
        var caRequest = new CertificateRequest(
            "CN=Test CA",
            caKey,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        caRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        var caCertificate = caRequest.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(1));

        var leafKey = RSA.Create(2048);
        var leafRequest = new CertificateRequest(
            "CN=verifier.example.com",
            leafKey,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        leafRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));

        var serialNumber = new byte[8];
        RandomNumberGenerator.Fill(serialNumber);
        var leafCertificate = leafRequest.Create(
            caCertificate,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddMonths(6),
            serialNumber);

        return new CertificateChain(leafCertificate, leafKey, caCertificate);
    }

    private sealed record CertificateChain(X509Certificate2 Leaf, RSA LeafKey, X509Certificate2 Ca);
}
