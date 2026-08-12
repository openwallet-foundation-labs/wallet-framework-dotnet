using System.Net;
using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.Protected;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.StatusList;
using Xunit;

namespace WalletFramework.Core.Tests.StatusList;

public class StatusListTests
{
    private const string RequestUriResponseWithBitSize1 =
        "eyJraWQiOiJkZjY5ODA1NDVlYWQwNTY3NTAwMGUyOGFiZDJhNTE0ZSIsInR5cCI6IkpXVCIsImFsZyI6IkVTMjU2In0.eyJzdWIiOiJodHRwczovL3NvbWUuaW8vc3RhdHVzLWxpc3RzP3JlZ2lzdHJ5SWQ9YmVlYTlmMTEtZGQ5ZC00MWUyLTljY2ItY2EzMGMxYjE5ZjRkIiwic3RhdHVzX2xpc3QiOnsiYml0cyI6MSwibHN0IjoiZUp4am1DakF5QUFBQW5zQW93PT0ifSwiaXNzIjoiaHR0cHM6Ly9zb21lLmlvIiwiZXhwIjoxNzMzMjE2MDY1LCJpYXQiOjE3MzMyMTI0NjV9.1osUmaMs9fBCNrpgVPNZa-Cr1f-ttYEpERQjen1xsxP2ePnoWzo44yUwPZZjojqDrgxRWaWC6lAtkk9xIZJhyg";
    
    private const string RequestUriResponseWithBitSize2 =
        "eyJraWQiOiJkZjY5ODA1NDVlYWQwNTY3NTAwMGUyOGFiZDJhNTE0ZSIsInR5cCI6IkpXVCIsImFsZyI6IkVTMjU2In0.eyJzdWIiOiJodHRwczovL3NvbWUuaW8vc3RhdHVzLWxpc3RzP3JlZ2lzdHJ5SWQ9YmVlYTlmMTEtZGQ5ZC00MWUyLTljY2ItY2EzMGMxYjE5ZjRkIiwic3RhdHVzX2xpc3QiOnsiYml0cyI6MiwibHN0IjoiZUp4am1DakF5QUFBQW5zQW93PT0ifSwiaXNzIjoiaHR0cHM6Ly9zb21lLmlvIiwiZXhwIjoxNzMzMjE2MDY1LCJpYXQiOjE3MzMyMTI0NjV9.SUHqhh69zvekBxtHo87p45ouYmOrpASqHkfgPisgLqqb7bo8_oXzvN_AS9huZpn5FA-wqTIoZgaBXBAKztIIag";

    private const string RequestUriResponseWithBitSize4 =
        "eyJraWQiOiJkZjY5ODA1NDVlYWQwNTY3NTAwMGUyOGFiZDJhNTE0ZSIsInR5cCI6IkpXVCIsImFsZyI6IkVTMjU2In0.eyJzdWIiOiJodHRwczovL3NvbWUuaW8vc3RhdHVzLWxpc3RzP3JlZ2lzdHJ5SWQ9YmVlYTlmMTEtZGQ5ZC00MWUyLTljY2ItY2EzMGMxYjE5ZjRkIiwic3RhdHVzX2xpc3QiOnsiYml0cyI6NCwibHN0IjoiZUp4am1DakF5QUFBQW5zQW93PT0ifSwiaXNzIjoiaHR0cHM6Ly9zb21lLmlvIiwiZXhwIjoxNzMzMjE2MDY1LCJpYXQiOjE3MzMyMTI0NjV9.XcKo7rYVWm0u6LyIUs3mSeutME_q5Qy1l_AHNA6UxgaywCR7n7MrCVd8OirSbRl68ImJabdisdpUP72hlQNw9g";

    private const string RequestUriResponseWithBitSize8 =
        "eyJraWQiOiJkZjY5ODA1NDVlYWQwNTY3NTAwMGUyOGFiZDJhNTE0ZSIsInR5cCI6IkpXVCIsImFsZyI6IkVTMjU2In0.eyJzdWIiOiJodHRwczovL3NvbWUuaW8vc3RhdHVzLWxpc3RzP3JlZ2lzdHJ5SWQ9YmVlYTlmMTEtZGQ5ZC00MWUyLTljY2ItY2EzMGMxYjE5ZjRkIiwic3RhdHVzX2xpc3QiOnsiYml0cyI6OCwibHN0IjoiZUp4am1DakF5QUFBQW5zQW93PT0ifSwiaXNzIjoiaHR0cHM6Ly9zb21lLmlvIiwiZXhwIjoxNzMzMjE2MDY1LCJpYXQiOjE3MzMyMTI0NjV9.Y8WWjgxkkUw7_UBXALxYPvenOXuPTyjTj09VKB-o5cGDtKZj-Lw66ZQFU0eOFvwXZrWzi8XL5ecfyNknzLzfOw";

    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<ILogger<StatusListService>> _loggerMock = new();
    
    [Theory]
    [InlineData(6, RequestUriResponseWithBitSize1, CredentialState.Active)]
    [InlineData(8, RequestUriResponseWithBitSize1, CredentialState.Revoked)]
    [InlineData(5, RequestUriResponseWithBitSize2, CredentialState.Active)]
    [InlineData(6, RequestUriResponseWithBitSize2, CredentialState.Revoked)]
    [InlineData(4, RequestUriResponseWithBitSize4, CredentialState.Active)]
    [InlineData(5, RequestUriResponseWithBitSize4, CredentialState.Revoked)]
    [InlineData(4, RequestUriResponseWithBitSize8, CredentialState.Active)]
    [InlineData(3, RequestUriResponseWithBitSize8, CredentialState.Revoked)]
    public void CanCreateStatus(int statusListIntex, string httpResponseJwt, CredentialState expectedState)
    {
        // Act
        var sut = StatusListStateReader.GetState(httpResponseJwt, statusListIntex);
        
        // Assert
        Assert.True(sut.Match(state => state == expectedState, () => false));
    }

    [Fact]
    public async Task ReturnsNoneWhenRequestFails()
    {
        // Arrange
        SetupHttpException(new HttpRequestException("Network unavailable"));
        var status = new StatusListEntry(0, "https://example.com");

        // Act
        var state = await CreateService().GetState(status);

        // Assert
        state.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnsNoneWhenRequestTimesOut()
    {
        // Arrange
        SetupHttpException(new OperationCanceledException("Request timed out"));
        var status = new StatusListEntry(0, "https://example.com");

        // Act
        var state = await CreateService().GetState(status);

        // Assert
        state.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnsNoneWhenResponseIsNotSuccessful()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.ServiceUnavailable, string.Empty);
        var status = new StatusListEntry(0, "https://example.com");

        // Act
        var state = await CreateService().GetState(status);

        // Assert
        state.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnsNoneWhenResponseCannotBeParsed()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "not-a-token");
        var status = new StatusListEntry(0, "https://example.com");

        // Act
        var state = await CreateService().GetState(status);

        // Assert
        state.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnsNoneWhenStatusListCannotBeDecompressed()
    {
        // Arrange
        var token = CreateToken("""{"bits":1,"lst":"bad"}""");
        SetupHttpResponse(HttpStatusCode.OK, token);
        var status = new StatusListEntry(0, "https://example.com");

        // Act
        var state = await CreateService().GetState(status);

        // Assert
        state.IsNone.Should().BeTrue();
    }

    private StatusListService CreateService() =>
        new(_httpClientFactoryMock.Object, _loggerMock.Object);

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(content)
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => httpResponseMessage);

        SetupHttpClient();
    }

    private void SetupHttpException(Exception exception)
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exception);

        SetupHttpClient();
    }

    private void SetupHttpClient()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
    }

    private static string CreateToken(string statusListJson)
    {
        var header = Base64UrlEncode("""{"alg":"none"}""");
        var payload = Base64UrlEncode($$"""{"status_list":{{statusListJson}}}""");

        return $"{header}.{payload}.";
    }

    private static string Base64UrlEncode(string value) =>
        Base64UrlEncoder.Encode(global::System.Text.Encoding.UTF8.GetBytes(value));
}
