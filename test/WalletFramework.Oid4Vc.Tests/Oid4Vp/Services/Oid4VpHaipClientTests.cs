using System.Net;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Services;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Services
{
    public class Oid4VpHaipClientTests 
    {
        private const string AuthRequestWithRequestUri =
            "haip://?client_id=https%3A%2F%2Fnc-sd-jwt.lambda.d3f.me%2Findex.php%2Fapps%2Fssi_login%2Foidc%2Fcallback&request_uri=https%3A%2F%2Fnc-sd-jwt.lambda.d3f.me%2Findex.php%2Fapps%2Fssi_login%2Foidc%2Frequestobject%2F4ba20ad0cb08545830aa549ab4305c03";

        private const string RequestUriResponse =
            "eyJhbGciOiJFUzI1NiJ9.eyJyZXNwb25zZV90eXBlIjoidnBfdG9rZW4iLCJyZXNwb25zZV9tb2RlIjoiZGlyZWN0X3Bvc3QiLCJjbGllbnRfaWRfc2NoZW1lIjoicmVkaXJlY3RfdXJpIiwiY2xpZW50X2lkIjoiaHR0cHM6Ly92ZXJpZmllci5jb20vcHJlc2VudGF0aW9uL2F1dGhvcml6YXRpb24tcmVzcG9uc2UiLCJjbGllbnRfbWV0YWRhdGFfdXJpIjoiaHR0cHM6Ly92ZXJpZmllci5jb20vbWV0YWRhdGEvMTIzNCIsInJlc3BvbnNlX3VyaSI6Imh0dHBzOi8vdmVyaWZpZXIuY29tL3ByZXNlbnRhdGlvbi9hdXRob3JpemF0aW9uLXJlc3BvbnNlIiwibm9uY2UiOiI4NzU1NDc4NDI2MDI4MDI4MDQ0MjA5MjE4NDE3MTI3NDEzMjQ1OCIsInByZXNlbnRhdGlvbl9kZWZpbml0aW9uIjp7ImlkIjoiNGRkMWMyNmEtMmY0Ni00M2FlLWE3MTEtNzA4ODhjOTNmYjRmIiwiaW5wdXRfZGVzY3JpcHRvcnMiOlt7ImlkIjoiTmV4dGNsb3VkQ3JlZGVudGlhbCIsImZvcm1hdCI6eyJ2YytzZC1qd3QiOnsicHJvb2ZfdHlwZSI6WyJKc29uV2ViU2lnbmF0dXJlMjAyMCJdfX0sImNvbnN0cmFpbnRzIjp7ImxpbWl0X2Rpc2Nsb3N1cmUiOiJyZXF1aXJlZCIsImZpZWxkcyI6W3sicGF0aCI6WyIkLnR5cGUiXSwiZmlsdGVyIjp7InR5cGUiOiJzdHJpbmciLCJjb25zdCI6IlZlcmlmaWVkRU1haWwifX0seyJwYXRoIjpbIiQuY3JlZGVudGlhbFN1YmplY3QuZW1haWwiXX1dfX1dfX0.";

        private static string VerifierMetadataResponse =>
            new JObject
                {
                    ["logo_uri"] = "https://some.de/logo",
                    ["client_name"] = "Some Verifier",
                    ["client_uri"] = "https://some.de",
                    ["contacts"] = new JArray("Any contact"),
                    ["redirect_uris"] = new JArray("https://verifier.com/redirect-uri"),
                    ["policy_uri"] = "https://some.de/policy",
                    ["tos_uri"] = "https://some.de/tos",
                }
                .ToString();
        
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new Mock<IHttpClientFactory>();

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
            
            var verifierMetadataResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(VerifierMetadataResponse)
            };
            
            SetupHttpClientSequence(httpResponseMessage, verifierMetadataResponseMessage);
            
            _oid4VpHaipClient = new Oid4VpHaipClient(_httpClientFactoryMock.Object, new PexService());

            // Act
            var authorizationRequest = await _oid4VpHaipClient.ProcessAuthorizationRequestAsync(
                HaipAuthorizationRequestUri.FromUri(new Uri(AuthRequestWithRequestUri))
            );

            // Assert
            authorizationRequest.ClientId.Should().Be("https://verifier.com/presentation/authorization-response");
            authorizationRequest.ResponseUri.Should().Be("https://verifier.com/presentation/authorization-response");
            authorizationRequest.Nonce.Should().Be("87554784260280280442092184171274132458");
            authorizationRequest.PresentationDefinition.Id.Should().Be("4dd1c26a-2f46-43ae-a711-70888c93fb4f");
            authorizationRequest.ClientMetadata.ClientName.Should().Be("Some Verifier");
            authorizationRequest.ClientMetadata.ClientUri.Should().Be("https://some.de");
            authorizationRequest.ClientMetadata.Contacts.First().Should().Be("Any contact");
            authorizationRequest.ClientMetadata.LogoUri.Should().Be("https://some.de/logo");
            authorizationRequest.ClientMetadata.PolicyUri.Should().Be("https://some.de/policy");
            authorizationRequest.ClientMetadata.TosUri.Should().Be("https://some.de/tos");
            authorizationRequest.ClientMetadata.RedirectUris.First().Should().Be("https://verifier.com/redirect-uri");

            var inputDescriptor = authorizationRequest.PresentationDefinition.InputDescriptors.First();

            inputDescriptor.Id.Should().Be("NextcloudCredential");

            inputDescriptor.Formats.First().Key.Should().Be("vc+sd-jwt");
            inputDescriptor.Formats.First().Value.ProofTypes.First().Should().Be("JsonWebSignature2020");

            inputDescriptor.Constraints.LimitDisclosure.Should().Be("required");

            inputDescriptor.Constraints.Fields!.First().Filter!.Type.Should().Be("string");
            inputDescriptor.Constraints.Fields!.First().Filter!.Const.Should().Be("VerifiedEMail");
            inputDescriptor.Constraints.Fields!.First().Path.First().Should().Be("$.type");

            inputDescriptor.Constraints.Fields![1].Path.First().Should().Be("$.credentialSubject.email");
        }
        
        private void SetupHttpClientSequence(params HttpResponseMessage[] responses)
        {
            var responseQueue = new Queue<HttpResponseMessage>(responses);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => responseQueue.Dequeue());

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        }
    }
}
