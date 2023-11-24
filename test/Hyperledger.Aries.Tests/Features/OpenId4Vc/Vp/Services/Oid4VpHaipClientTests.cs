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
            "eyJhbGciOiJub25lIn0.ew0KICAicmVzcG9uc2VfdHlwZSI6ICJ2cF90b2tlbiIsDQogICJyZXNwb25zZV9tb2RlIjogImRpcmVjdF9wb3N0IiwNCiAgImNsaWVudF9pZCI6ICJodHRwczovL25jLXNkLWp3dC5sYW1iZGEuZDNmLm1lL2luZGV4LnBocC9hcHBzL3NzaV9sb2dpbi9vaWRjL2NhbGxiYWNrIiwNCiAgInJlc3BvbnNlX3VyaSI6ICJodHRwczovL25jLXNkLWp3dC5sYW1iZGEuZDNmLm1lL2luZGV4LnBocC9hcHBzL3NzaV9sb2dpbi9vaWRjL2NhbGxiYWNrIiwNCiAgIm5vbmNlIjogIjg3NTU0Nzg0MjYwMjgwMjgwNDQyMDkyMTg0MTcxMjc0MTMyNDU4IiwNCiAgInByZXNlbnRhdGlvbl9kZWZpbml0aW9uIjogew0KICAgICJpZCI6ICI0ZGQxYzI2YS0yZjQ2LTQzYWUtYTcxMS03MDg4OGM5M2ZiNGYiLA0KICAgICJpbnB1dF9kZXNjcmlwdG9ycyI6IFsNCiAgICAgIHsNCiAgICAgICAgImlkIjogIk5leHRjbG91ZENyZWRlbnRpYWwiLA0KICAgICAgICAiZm9ybWF0Ijogew0KICAgICAgICAgICJ2YytzZC1qd3QiOiB7DQogICAgICAgICAgICAicHJvb2ZfdHlwZSI6IFsNCiAgICAgICAgICAgICAgIkpzb25XZWJTaWduYXR1cmUyMDIwIg0KICAgICAgICAgICAgXQ0KICAgICAgICAgIH0NCiAgICAgICAgfSwNCiAgICAgICAgImNvbnN0cmFpbnRzIjogew0KICAgICAgICAgICJsaW1pdF9kaXNjbG9zdXJlIjogInJlcXVpcmVkIiwNCiAgICAgICAgICAiZmllbGRzIjogWw0KICAgICAgICAgICAgew0KICAgICAgICAgICAgICAicGF0aCI6IFsNCiAgICAgICAgICAgICAgICAiJC50eXBlIg0KICAgICAgICAgICAgICBdLA0KICAgICAgICAgICAgICAiZmlsdGVyIjogew0KICAgICAgICAgICAgICAgICJ0eXBlIjogInN0cmluZyIsDQogICAgICAgICAgICAgICAgImNvbnN0IjogIlZlcmlmaWVkRU1haWwiDQogICAgICAgICAgICAgIH0NCiAgICAgICAgICAgIH0sDQogICAgICAgICAgICB7DQogICAgICAgICAgICAgICJwYXRoIjogWw0KICAgICAgICAgICAgICAgICIkLmNyZWRlbnRpYWxTdWJqZWN0LmVtYWlsIg0KICAgICAgICAgICAgICBdDQogICAgICAgICAgICB9DQogICAgICAgICAgXQ0KICAgICAgICB9DQogICAgICB9DQogICAgXQ0KICB9DQp9.";

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
