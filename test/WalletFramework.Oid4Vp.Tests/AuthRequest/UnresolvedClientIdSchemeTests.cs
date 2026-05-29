using System.Net;
using FluentAssertions;
using LanguageExt;
using Moq;
using Moq.Protected;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vp.Models;
using WalletFramework.Oid4Vp.RelyingPartyAuthentication;
using WalletFramework.Oid4Vp.RelyingPartyAuthentication.Abstractions;
using WalletFramework.Oid4Vp.Services;
using static WalletFramework.Oid4Vp.Tests.AuthRequest.Samples.UnresolvedClientIdSchemeSamples;

namespace WalletFramework.Oid4Vp.Tests.AuthRequest;

public class UnresolvedClientIdSchemeTests
{
    private const string RequestUri = "https://verifier.example.com/request";
    private const string ResponseUri = "https://verifier.example.com/response";

    [Fact]
    public async Task A_Referenced_Request_Without_A_Client_Id_Is_Refused_As_Unsupported()
    {
        var service = CreateServiceReturning(RequestObjectWithoutClientId());

        var result = await service.GetAuthorizationRequest(ReferenceUri());

        CancellationOf(result).Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("is not supported");
    }

    [Fact]
    public async Task A_Referenced_Request_With_An_Unsupported_Client_Id_Prefix_Is_Refused_As_Unsupported()
    {
        var service = CreateServiceReturning(RequestObjectWithUnsupportedClientIdPrefix());

        var result = await service.GetAuthorizationRequest(ReferenceUri());

        CancellationOf(result).Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("is not supported");
    }

    [Fact]
    public async Task A_Request_By_Value_With_An_Unsupported_Client_Id_Prefix_Is_Refused_As_Unsupported()
    {
        var service = CreateServiceReturning("unused");

        var result = await service.GetAuthorizationRequest(ValueUriWithUnsupportedClientIdPrefix());

        CancellationOf(result).Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("is not supported");
    }

    private static AuthorizationRequestCancellation CancellationOf(
        Validation<AuthorizationRequestCancellation, AuthorizationRequest> result) =>
        result.Match(
            _ => throw new InvalidOperationException("Expected a cancellation but the request succeeded"),
            failures => failures.First());

    private static AuthorizationRequestUri ReferenceUri()
    {
        var uri = new Uri($"openid4vp://?client_id=x509_hash:placeholder&request_uri={Uri.EscapeDataString(RequestUri)}");
        return AuthorizationRequestUri.FromUri(uri).UnwrapOrThrow();
    }

    private static AuthorizationRequestUri ValueUriWithUnsupportedClientIdPrefix()
    {
        var query =
            "client_id=unknown_scheme:verifier.example.com" +
            "&nonce=test-nonce-value" +
            "&scope=openid" +
            "&response_mode=direct_post.jwt" +
            $"&response_uri={Uri.EscapeDataString(ResponseUri)}";

        var uri = new Uri($"openid4vp://?{query}");
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
