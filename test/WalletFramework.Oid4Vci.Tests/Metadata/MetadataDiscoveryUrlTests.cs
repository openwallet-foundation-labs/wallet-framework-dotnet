using FluentAssertions;
using WalletFramework.Oid4Vci.Metadata;
using Xunit;

namespace WalletFramework.Oid4Vci.Tests.Metadata;

public class MetadataDiscoveryUrlTests
{
    [Theory(DisplayName = "credential issuer metadata discovery preserves the issuer path shape")]
    [InlineData(
        "https://issuer.example.com",
        "https://issuer.example.com/.well-known/openid-credential-issuer")]
    [InlineData(
        "https://issuer.example.com/",
        "https://issuer.example.com/.well-known/openid-credential-issuer")]
    [InlineData(
        "https://issuer.example.com/test/a/sdjwtEncrypted",
        "https://issuer.example.com/.well-known/openid-credential-issuer/test/a/sdjwtEncrypted")]
    [InlineData(
        "https://issuer.example.com/test/a/sdjwtEncrypted/",
        "https://issuer.example.com/.well-known/openid-credential-issuer/test/a/sdjwtEncrypted/")]
    [InlineData(
        "https://issuer.example.com/test/a/sdjwtEncrypted/?query=value#fragment",
        "https://issuer.example.com/.well-known/openid-credential-issuer/test/a/sdjwtEncrypted/")]
    public void CredentialIssuerMetadataDiscoveryPreservesTheIssuerPathShape(
        string issuerIdentifier,
        string expectedMetadataUrl)
    {
        var issuer = new Uri(issuerIdentifier);

        var metadataUrl = MetadataDiscoveryUrl.ForCredentialIssuer(issuer);

        metadataUrl.Should().Be(new Uri(expectedMetadataUrl));
    }

    [Theory(DisplayName = "authorization server metadata discovery preserves the issuer path shape")]
    [InlineData(
        "https://issuer.example.com",
        "https://issuer.example.com/.well-known/oauth-authorization-server")]
    [InlineData(
        "https://issuer.example.com/",
        "https://issuer.example.com/.well-known/oauth-authorization-server")]
    [InlineData(
        "https://issuer.example.com/test/a/sdjwtEncrypted",
        "https://issuer.example.com/.well-known/oauth-authorization-server/test/a/sdjwtEncrypted")]
    [InlineData(
        "https://issuer.example.com/test/a/sdjwtEncrypted/",
        "https://issuer.example.com/.well-known/oauth-authorization-server/test/a/sdjwtEncrypted/")]
    [InlineData(
        "https://issuer.example.com/test/a/sdjwtEncrypted/?query=value#fragment",
        "https://issuer.example.com/.well-known/oauth-authorization-server/test/a/sdjwtEncrypted/")]
    public void AuthorizationServerMetadataDiscoveryPreservesTheIssuerPathShape(
        string issuerIdentifier,
        string expectedMetadataUrl)
    {
        var issuer = new Uri(issuerIdentifier);

        var metadataUrl = MetadataDiscoveryUrl.ForAuthorizationServer(issuer);

        metadataUrl.Should().Be(new Uri(expectedMetadataUrl));
    }
}
