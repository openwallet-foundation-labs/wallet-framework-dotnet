using System.Net;
using Moq;
using Moq.Protected;
using WalletFramework.Oid4Vp.Models;
using WalletFramework.Oid4Vp.RelyingPartyAuthentication;
using WalletFramework.Oid4Vp.RelyingPartyAuthentication.Abstractions;
using WalletFramework.Oid4Vp.Services;

namespace WalletFramework.Oid4Vp.Tests.AuthRequest;

internal static class AuthorizationRequestServiceFactory
{
    private const string SendAsyncMethodName = "SendAsync";

    internal static AuthorizationRequestService CreateServiceReturning(string requestObjectJwt)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                SendAsyncMethodName,
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
