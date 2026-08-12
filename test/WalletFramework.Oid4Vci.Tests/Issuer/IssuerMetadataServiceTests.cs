using System.Net;
using FluentAssertions;
using Moq;
using Moq.Protected;
using WalletFramework.Oid4Vci.Issuer.Implementations;
using WalletFramework.Oid4Vci.Tests.Issuer.Samples;
using WalletFramework.Oid4Vci.Tests.Localization.Samples;
using Xunit;

namespace WalletFramework.Oid4Vci.Tests.Issuer;

public class IssuerMetadataServiceTests
{
    [Theory(DisplayName = "credential issuer metadata is discovered through the authority well-known path")]
    [InlineData(
        "https://issuer.example.com/tenant",
        "https://issuer.example.com/.well-known/openid-credential-issuer/tenant")]
    [InlineData(
        "https://issuer.example.com/tenant/",
        "https://issuer.example.com/.well-known/openid-credential-issuer/tenant/")]
    [InlineData(
        "https://issuer.example.com",
        "https://issuer.example.com/.well-known/openid-credential-issuer")]
    public async Task CredentialIssuerMetadataIsDiscoveredThroughTheAuthorityWellKnownPath(
        string issuer,
        string expectedMetadataUrl)
    {
        var requestedUri = default(Uri);
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => requestedUri = request.RequestUri)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(IssuerMetadataSample.EncodedAsJson.ToString())
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(factory => factory.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);
        var sut = new IssuerMetadataService(httpClientFactoryMock.Object);

        await sut.ProcessMetadata(new Uri(issuer), LocaleSample.English);

        requestedUri.Should().Be(new Uri(expectedMetadataUrl));
    }
}
