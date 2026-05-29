using System.Net;
using FluentAssertions;
using Moq;
using Moq.Protected;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vp.Models;
using WalletFramework.Oid4Vp.RelyingPartyAuthentication;
using WalletFramework.Oid4Vp.RelyingPartyAuthentication.Abstractions;
using WalletFramework.Oid4Vp.Services;
using static WalletFramework.Oid4Vp.Tests.AuthRequest.Samples.X509HashSamples;

namespace WalletFramework.Oid4Vp.Tests.AuthRequest;

public class X509HashRequestProcessingTests
{
    private const string RequestUri = "https://verifier.example.com/request";

    [Fact]
    public async Task A_Signed_Request_With_A_Matching_Certificate_Hash_Is_Processed_Successfully()
    {
        var service = CreateServiceReturning(SignedRequestObjectWithMatchingCertificateHash());

        var result = await service.GetAuthorizationRequest(ReferenceUri());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task A_Signed_Request_With_A_Mismatched_Certificate_Hash_Is_Cancelled_Without_Throwing()
    {
        var service = CreateServiceReturning(SignedRequestObjectWithMismatchedCertificateHash());

        var result = await service.GetAuthorizationRequest(ReferenceUri());

        result.IsSuccess.Should().BeFalse();
    }

    private static AuthorizationRequestUri ReferenceUri()
    {
        var uri = new Uri($"openid4vp://?client_id=x509_hash:placeholder&request_uri={Uri.EscapeDataString(RequestUri)}");
        return AuthorizationRequestUri.FromUri(uri).UnwrapOrThrow();
    }

    private static AuthorizationRequestService CreateServiceReturning(string requestObjectJwt)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(requestObjectJwt)
            });

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(factory => factory.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler.Object));

        var rpAuthService = new Mock<IRpAuthService>();
        rpAuthService
            .Setup(service => service.Authenticate(It.IsAny<RequestObject>()))
            .ReturnsAsync(RpAuthResult.GetWithLevelUnknown());

        return new AuthorizationRequestService(httpClientFactory.Object, rpAuthService.Object);
    }
}
