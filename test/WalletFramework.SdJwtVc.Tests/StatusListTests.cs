using System.Net;
using Moq;
using Moq.Protected;
using WalletFramework.Core.Credentials;
using WalletFramework.SdJwtVc.Models.StatusList;
using WalletFramework.SdJwtVc.Services;

namespace WalletFramework.SdJwtVc.Tests;

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
    
    [Theory]
    [InlineData(6, RequestUriResponseWithBitSize1, CredentialState.Active)]
    [InlineData(8, RequestUriResponseWithBitSize1, CredentialState.Revoked)]
    [InlineData(5, RequestUriResponseWithBitSize2, CredentialState.Active)]
    [InlineData(6, RequestUriResponseWithBitSize2, CredentialState.Revoked)]
    [InlineData(4, RequestUriResponseWithBitSize4, CredentialState.Active)]
    [InlineData(5, RequestUriResponseWithBitSize4, CredentialState.Revoked)]
    [InlineData(4, RequestUriResponseWithBitSize8, CredentialState.Active)]
    [InlineData(3, RequestUriResponseWithBitSize8, CredentialState.Revoked)]
    public async Task CanCreateStatus(int statusListIntex, string httpResponseJwt, CredentialState expectedState)
    {
        // Arrange
        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(httpResponseJwt)
        };
        
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => httpResponseMessage);

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        
        var status = new StatusListEntry(statusListIntex, "https://example.com");
        
        // Act
        var sut = await new StatusListService(_httpClientFactoryMock.Object).GetState(status);
        
        // Assert
        Assert.True(sut.Match(state => state == expectedState, () => false));
    }
}
