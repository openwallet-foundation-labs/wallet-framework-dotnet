using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hyperledger.Aries.Features.OpenId4Vc.Vp.Models;
using Hyperledger.Aries.Features.OpenId4Vc.Vp.Services;
using Hyperledger.Aries.Features.Pex.Services;
using Moq;
using Moq.Protected;
using Xunit;

namespace Hyperledger.Aries.Tests.Features.OpenId4Vc.Vp.Services
{
    public class Oid4VpHaipClientTests
    {
        private const string AuthRequestWithRequestUri =
            "haip://?client_id=https%3A%2F%2Fnc-sd-jwt.lambda.d3f.me%2Findex.php%2Fapps%2Fssi_login%2Foidc%2Fcallback&request_uri=https%3A%2F%2Fnc-sd-jwt.lambda.d3f.me%2Findex.php%2Fapps%2Fssi_login%2Foidc%2Frequestobject%2F4ba20ad0cb08545830aa549ab4305c03";

        private const string RequestUriResponse =
            "eyJhbGciOiJub25lIn0.ew0KICAicmVzcG9uc2VfdHlwZSI6ICJ2cF90b2tlbiIsDQogICJyZXNwb25zZV9tb2RlIjogImRpcmVjdF9wb3N0IiwNCiAgImNsaWVudF9pZCI6ICJodHRwczovL25jLXNkLWp3dC5sYW1iZGEuZDNmLm1lL2luZGV4LnBocC9hcHBzL3NzaV9sb2dpbi9vaWRjL2NhbGxiYWNrIiwNCiAgInJlc3BvbnNlX3VyaSI6ICJodHRwczovL25jLXNkLWp3dC5sYW1iZGEuZDNmLm1lL2luZGV4LnBocC9hcHBzL3NzaV9sb2dpbi9vaWRjL2NhbGxiYWNrIiwNCiAgIm5vbmNlIjogIjg3NTU0Nzg0MjYwMjgwMjgwNDQyMDkyMTg0MTcxMjc0MTMyNDU4IiwNCiAgInByZXNlbnRhdGlvbl9kZWZpbml0aW9uIjogew0KICAgICJpZCI6ICI0ZGQxYzI2YS0yZjQ2LTQzYWUtYTcxMS03MDg4OGM5M2ZiNGYiLA0KICAgICJpbnB1dF9kZXNjcmlwdG9ycyI6IFsNCiAgICAgIHsNCiAgICAgICAgImlkIjogIk5leHRjbG91ZENyZWRlbnRpYWwiLA0KICAgICAgICAiZm9ybWF0Ijogew0KICAgICAgICAgICJ2YytzZC1qd3QiOiB7DQogICAgICAgICAgICAicHJvb2ZfdHlwZSI6IFsNCiAgICAgICAgICAgICAgIkpzb25XZWJTaWduYXR1cmUyMDIwIg0KICAgICAgICAgICAgXQ0KICAgICAgICAgIH0NCiAgICAgICAgfSwNCiAgICAgICAgImNvbnN0cmFpbnRzIjogew0KICAgICAgICAgICJsaW1pdF9kaXNjbG9zdXJlIjogInJlcXVpcmVkIiwNCiAgICAgICAgICAiZmllbGRzIjogWw0KICAgICAgICAgICAgew0KICAgICAgICAgICAgICAicGF0aCI6IFsNCiAgICAgICAgICAgICAgICAiJC50eXBlIg0KICAgICAgICAgICAgICBdLA0KICAgICAgICAgICAgICAiZmlsdGVyIjogew0KICAgICAgICAgICAgICAgICJ0eXBlIjogInN0cmluZyIsDQogICAgICAgICAgICAgICAgImNvbnN0IjogIlZlcmlmaWVkRU1haWwiDQogICAgICAgICAgICAgIH0NCiAgICAgICAgICAgIH0sDQogICAgICAgICAgICB7DQogICAgICAgICAgICAgICJwYXRoIjogWw0KICAgICAgICAgICAgICAgICIkLmNyZWRlbnRpYWxTdWJqZWN0LmVtYWlsIg0KICAgICAgICAgICAgICBdDQogICAgICAgICAgICB9DQogICAgICAgICAgXQ0KICAgICAgICB9DQogICAgICB9DQogICAgXQ0KICB9DQp9.";

        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();

        private Oid4VpHaipClient _oid4VpHaipClient;

        [Fact]
        public async Task CanProcessAuthorizationRequest()
        {
            // Arrange
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(RequestUriResponse)
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(() => httpResponseMessage);

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            _httpClientFactoryMock.Setup(
                    f => f.CreateClient(It.IsAny<string>())
                )
                .Returns(httpClient);

            _oid4VpHaipClient = new Oid4VpHaipClient(_httpClientFactoryMock.Object, new PexService());

            // Act
            var authorizationRequest = await _oid4VpHaipClient.ProcessAuthorizationRequestAsync(
                HaipAuthorizationRequestUri.FromUri(new Uri(AuthRequestWithRequestUri))
            );

            // Assert
            authorizationRequest.ResponseType.Should().Be("vp_token");
            authorizationRequest.ClientId.Should().Be("https://verifier.com/presentation/authorization-response");
            authorizationRequest.ResponseUri.Should().Be("https://verifier.com/presentation/authorization-response");
            authorizationRequest.Nonce.Should().Be("87554784260280280442092184171274132458");
            authorizationRequest.ResponseMode.Should().Be("direct_post");
            authorizationRequest.PresentationDefinition.Id.Should().Be("4dd1c26a-2f46-43ae-a711-70888c93fb4f");

            var inputDescriptor = authorizationRequest.PresentationDefinition.InputDescriptors.First();

            inputDescriptor.Id.Should().Be("NextCloudCredential");

            inputDescriptor.Formats.First().Key.Should().Be("vc+sd-jwt");
            inputDescriptor.Formats.First().Value.ProofTypes.First().Should().Be("JsonWebSignature2020");

            inputDescriptor.Constraints.LimitDisclosure.Should().Be("required");

            inputDescriptor.Constraints.Fields!.First().Filter!.Type.Should().Be("string");
            inputDescriptor.Constraints.Fields!.First().Filter!.Const.Should().Be("VerifiedEmail");
            inputDescriptor.Constraints.Fields!.First().Path.First().Should().Be("$.type");

            inputDescriptor.Constraints.Fields![1].Path.First().Should().Be("$.credentialSubject.email");
        }
    }
}
