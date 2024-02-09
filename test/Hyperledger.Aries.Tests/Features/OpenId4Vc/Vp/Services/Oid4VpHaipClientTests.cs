using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hyperledger.Aries.Features.OpenId4Vc.Vp.Models;
using Hyperledger.Aries.Features.OpenId4Vc.Vp.Services;
using Hyperledger.Aries.Features.Pex.Models;
using Hyperledger.Aries.Features.Pex.Services;
using Hyperledger.Aries.Tests.Extensions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Hyperledger.Aries.Tests.Features.OpenId4Vc.Vp.Services
{
    public class Oid4VpHaipClientTests
    {
        private const string AuthRequestViaUri =
            "haip://?response_type=vp_token&nonce=87554784260280280442092184171274132458&client_id=https%3A%2F%2Fnc-sd-jwt.lambda.d3f.me%2Findex.php%2Fapps%2Fssi_login%2Foidc%2Fcallback&redirect_uri=https%3A%2F%2Fnc-sd-jwt.lambda.d3f.me%2Findex.php%2Fapps%2Fssi_login%2Foidc%2Fcallback&presentation_definition=%7B%22id%22%3A%224dd1c26a-2f46-43ae-a711-70888c93fb4f%22%2C%22input_descriptors%22%3A%5B%7B%22id%22%3A%22NextcloudCredential%22%2C%22format%22%3A%7B%22vc%2Bsd-jwt%22%3A%7B%22proof_type%22%3A%5B%22JsonWebSignature2020%22%5D%7D%7D%2C%22constraints%22%3A%7B%22limit_disclosure%22%3A%22required%22%2C%22fields%22%3A%5B%7B%22path%22%3A%5B%22%24.type%22%5D%2C%22filter%22%3A%7B%22type%22%3A%22string%22%2C%22const%22%3A%22VerifiedEMail%22%7D%7D%2C%7B%22path%22%3A%5B%22%24.credentialSubject.email%22%5D%7D%5D%7D%7D%5D%7D";

        private const string AuthRequestWithRequestUri =
            "haip://?client_id=https%3A%2F%2Fnc-sd-jwt.lambda.d3f.me%2Findex.php%2Fapps%2Fssi_login%2Foidc%2Fcallback&request_uri=https%3A%2F%2Fnc-sd-jwt.lambda.d3f.me%2Findex.php%2Fapps%2Fssi_login%2Foidc%2Frequestobject%2F4ba20ad0cb08545830aa549ab4305c03";

        private const string RequestUriResponse =
            "eyJhbGciOiJFUzI1NiJ9.eyJyZXNwb25zZV90eXBlIjoidnBfdG9rZW4iLCJyZXNwb25zZV9tb2RlIjoiZGlyZWN0X3Bvc3QiLCJjbGllbnRfaWQiOiJodHRwczovL3ZlcmlmaWVyLmNvbS9wcmVzZW50YXRpb24vYXV0aG9yaXphdGlvbi1yZXNwb25zZSIsInJlc3BvbnNlX3VyaSI6Imh0dHBzOi8vdmVyaWZpZXIuY29tL3ByZXNlbnRhdGlvbi9hdXRob3JpemF0aW9uLXJlc3BvbnNlIiwibm9uY2UiOiI4NzU1NDc4NDI2MDI4MDI4MDQ0MjA5MjE4NDE3MTI3NDEzMjQ1OCIsInByZXNlbnRhdGlvbl9kZWZpbml0aW9uIjp7ImlkIjoiNGRkMWMyNmEtMmY0Ni00M2FlLWE3MTEtNzA4ODhjOTNmYjRmIiwiaW5wdXRfZGVzY3JpcHRvcnMiOlt7ImlkIjoiTmV4dGNsb3VkQ3JlZGVudGlhbCIsImZvcm1hdCI6eyJ2YytzZC1qd3QiOnsicHJvb2ZfdHlwZSI6WyJKc29uV2ViU2lnbmF0dXJlMjAyMCJdfX0sImNvbnN0cmFpbnRzIjp7ImxpbWl0X2Rpc2Nsb3N1cmUiOiJyZXF1aXJlZCIsImZpZWxkcyI6W3sicGF0aCI6WyIkLnR5cGUiXSwiZmlsdGVyIjp7InR5cGUiOiJzdHJpbmciLCJjb25zdCI6IlZlcmlmaWVkRU1haWwifX0seyJwYXRoIjpbIiQuY3JlZGVudGlhbFN1YmplY3QuZW1haWwiXX1dfX1dfX0.";

        private Oid4VpHaipClient _oid4VpHaipClient;
        
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new Mock<IHttpClientFactory>();

        [Theory]
        [InlineData(AuthRequestWithRequestUri, RequestUriResponse)]
        public async Task CanProcessAuthorizationRequest(string authorizationRequestUri, string httpResponse)
        {
            // Arrange
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(httpResponse)
            };
            
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => httpResponseMessage);

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            _oid4VpHaipClient = new Oid4VpHaipClient(_httpClientFactoryMock.Object, new PexService());
            
            var expectedAuthorizationRequest = GetExpectedAuthorizationRequest();

            // Act
            var authorizationRequest = await _oid4VpHaipClient.ProcessAuthorizationRequestAsync(
                    HaipAuthorizationRequestUri.FromUri(new Uri(authorizationRequestUri)));

            // Assert
            authorizationRequest.Should().BeEquivalentTo(expectedAuthorizationRequest);
        }

        private AuthorizationRequest GetExpectedAuthorizationRequest()
        {
            var format = new Format();
            format.PrivateSet(x => x.ProofTypes, new[] { "JsonWebSignature2020" });
            
            var filter = new Filter();
            filter.PrivateSet(x => x.Type, "string");
            filter.PrivateSet(x => x.Const, "VerifiedEMail");
            
            var fieldOne = new Field();
            fieldOne.PrivateSet(x => x.Filter, filter);
            fieldOne.PrivateSet(x => x.Path, new [] {"$.type"});
            
            var fieldTwo = new Field();
            fieldTwo.PrivateSet(x => x.Path, new [] {"$.credentialSubject.email"});
            
            var constraints = new Constraints();
            constraints.PrivateSet(x => x.LimitDisclosure, "required");
            constraints.PrivateSet(x => x.Fields, new [] {fieldOne, fieldTwo});
            
            var inputDescriptor = new InputDescriptor();
            inputDescriptor.PrivateSet(x => x.Id, "NextcloudCredential");
            inputDescriptor.PrivateSet(x => x.Formats, new Dictionary<string, Format> { {"vc+sd-jwt", format }});
            inputDescriptor.PrivateSet(x => x.Constraints, constraints);
            
            var presentationDefinition = new PresentationDefinition();
            presentationDefinition.PrivateSet(x => x.Id, "4dd1c26a-2f46-43ae-a711-70888c93fb4f");
            presentationDefinition.PrivateSet(x => x.InputDescriptors, new[] { inputDescriptor });
            
            return new AuthorizationRequest()
            {
                ResponseType = "vp_token",
                ClientId = "https://verifier.com/presentation/authorization-response",
                ResponseUri = "https://verifier.com/presentation/authorization-response",
                Nonce = "87554784260280280442092184171274132458",
                ResponseMode = "direct_post",
                PresentationDefinition = presentationDefinition,
            };
        }
        
    }
}
