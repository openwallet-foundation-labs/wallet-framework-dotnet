using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hyperledger.Aries.Features.OpenId4Vc.KeyStore.Services;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialResponse;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Credential;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Credential.Attributes;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Issuer;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Services.Oid4VciClientService;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace Hyperledger.Aries.Tests.Features.OpenId4Vc.Vci.Services
{
    public class Oid4VciClientServiceTests
    {
        private const string AuthServerMetadataWithoutDpop =
            "{\"issuer\":\"https://issuer.io\",\"token_endpoint\":\"https://issuer.io/token\",\"token_endpoint_auth_methods_supported\":[\"urn:ietf:params:oauth:client-assertion-type:verifiable-presentation\"],\"response_types_supported\":[\"urn:ietf:params:oauth:grant-type:pre-authorized_code\"]}\n";

        private const string AuthServerMetadataWithDpop =
            "{\"issuer\":\"https://issuer.io\",\"token_endpoint\":\"https://issuer.io/token\",\"token_endpoint_auth_methods_supported\":[\"urn:ietf:params:oauth:client-assertion-type:verifiable-presentation\"],\"response_types_supported\":[\"urn:ietf:params:oauth:grant-type:pre-authorized_code\"],\"dpop_signing_alg_values_supported\":[\"ES256\"]}\n";
        
        private const string PreAuthorizedCode = "1234";

        private const string TokenResponseWithoutDpopSupport =
            "{\"access_token\":\"eyJhbGciOiJSUzI1NiIsInR5cCI6Ikp..sHQ\",\"token_type\":\"bearer\",\"expires_in\": 86400,\"c_nonce\": \"tZignsnFbp\",\"c_nonce_expires_in\":86400}";

        private const string TokenResponseWithDpopSupport =
            "{\"access_token\":\"eyJhbGciOiJSUzI1NiIsInR5cCI6Ikp..sHQ\",\"token_type\":\"DPoP\",\"expires_in\": 86400,\"c_nonce\": \"tZignsnFbp\",\"c_nonce_expires_in\":86400}";
        
        private const string DPopBadRequestTokenResponse = "{\"error\":\"use_dpop_nonce\"}";
        
        private const string Vct = "VerifiedEmail";

        private const string DPopNonce = "someRadnomNonceStringFromIssuer";
        
        private const string CredentialResponse = "{\"format\":\"vc+sd-jwt\",\"credential\":\"eyJhbGciOiAiRVMyNTYifQ.eyJfc2QiOlsiT0dfT2lCMk5ZS0JzTVhIOFVVb2luREhUT1h5VER1Z3JPdE94RFI2NF9ZcyIsIlQzbHRYQUFtODNJTXRUYkRTb1J2d1g2Tk10em1scV9ZWG9Vd1EwZDY0NEUiXSwiaXNzIjoiaHR0cHM6Ly9pc3N1ZXIuaW8vIiwiaWF0IjoxNTE2MjM5MDIyLCJ0eXBlIjoiVmVyaWZpZWRFbWFpbCIsImV4cCI6MTUxNjI0NzAyMiwiX3NkX2FsZyI6InNoYS0yNTYiLCJjbmYiOnsiandrIjp7Imt0eSI6IkVDIiwiY3J2IjoiUC0yNTYiLCJ4IjoiVENBRVIxOVp2dTNPSEY0ajRXNHZmU1ZvSElQMUlMaWxEbHM3dkNlR2VtYyIsInkiOiJaeGppV1diWk1RR0hWV0tWUTRoYlNJaXJzVmZ1ZWNDRTZ0NGpUOUYySFpRIn19LCJhbGciOiJFUzI1NiJ9.OVSoCqHZLgAPaYK27gJx6J1ejwskP62xIHryqc1ZJYOR8yZdicSF4KXBk5qgocWZdiqEsri5Q3sW69xIfbmXSA~WyJseVMxN1ZzenNGb3doaFBnY3VuOTFRIiwgImV4cCIsIDE1NDE0OTQ3MjRd~WyJaRmNwSWxTNlJ5eWV2U3JTeFdJbDZRIiwgImdpdmVuX25hbWUiLCAiSm9obiJd~WyJVSHVVVUNlOWZzNUdody1mZ0JJWi13IiwgImZhbWlseV9uYW1lIiwgIkRvZSJd~WyJ3ZnR5YkpzYktzVWJDay1XaWpaQ3RRIiwgImVtYWlsIiwgInRlc3RAZXhhbXBsZS5jb20iXQ\"}";

        private const string KeyBindingJwtKeyId = "someKbJwtKeyId";
        
        private const string DPopJwtKeyId = "someDpopJwtKeyId";
        
        private const string KbJwtMock = "someKeyBindingJwtMock";

        private const string TransactionCode = "someTransactionCode";
        
        private readonly HttpResponseMessage _authServerMetadataResponseWithoutDPopSupport = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(AuthServerMetadataWithoutDpop)
        };
        
        private readonly HttpResponseMessage _authServerMetadataResponseWithDPopSupport = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(AuthServerMetadataWithDpop)
        };
        
        private readonly HttpResponseMessage _credentialResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(CredentialResponse)
        };

        private readonly HttpResponseMessage _tokenResponseWithoutDpopSupport = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(TokenResponseWithoutDpopSupport)
        };
        
        private readonly HttpResponseMessage _tokenResponseWithDpopSupport = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(TokenResponseWithDpopSupport)
        };
        
        private readonly HttpResponseMessage _dPopBadRequestTokenResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent(DPopBadRequestTokenResponse),
            Headers = {{"DPoP-Nonce", DPopNonce}},
        };

        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        private readonly Mock<IKeyStore> _keyStoreMock = new Mock<IKeyStore>();

        private Oid4VciClientService _oid4VciClientService;

        private readonly OidIssuerMetadata _oidIssuerMetadata = new OidIssuerMetadata()
        {
            CredentialIssuer = "https://issuer.io",
            CredentialEndpoint = "https://issuer.io/credential",
            CredentialsSupported = new Dictionary<string, OidCredentialMetadata>
            {
                {
                    "VerifiedEmail", new OidCredentialMetadata
                    {
                        Format = "vc+sdjwt",
                        CredentialDefinition = new OidCredentialDefinition
                        {
                            Vct = Vct,
                            Claims = new Dictionary<string, OidClaim>()
                        }
                    }
                }
            }
        };

        [Fact]
        public async Task CanRequestCredentialWithoutDPopAsync()
        {
            //Arrange
            SetupKeyStoreGenerateKeySequence(KeyBindingJwtKeyId);
            _keyStoreMock.Setup(j =>
                    j.GenerateKbProofOfPossessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>()))
                .ReturnsAsync(KbJwtMock);
            
            SetupHttpClientSequence(_authServerMetadataResponseWithoutDPopSupport, _tokenResponseWithoutDpopSupport, _credentialResponse);
            
            var expectedCredentialResponse = JsonConvert.DeserializeObject<OidCredentialResponse>(CredentialResponse);
            
            //Act
            var actualCredentialResponse = 
                await _oid4VciClientService.RequestCredentialAsync(
                    _oidIssuerMetadata.CredentialsSupported.First().Value,
                    _oidIssuerMetadata,
                    PreAuthorizedCode,
                    TransactionCode
                    );
            
            //Assert
            actualCredentialResponse.Item1.Should().BeEquivalentTo(expectedCredentialResponse);
            actualCredentialResponse.Item2.Should().BeEquivalentTo(KeyBindingJwtKeyId);
        }
        
        [Fact]
        public async Task CanRequestCredentialWithDPoPAsync()
        {
            //Arrange
            const string dPopJwtMock = "someDPopJwtMock";
            
            SetupKeyStoreGenerateKeySequence(DPopJwtKeyId, KeyBindingJwtKeyId);
            _keyStoreMock.Setup(j =>
                    j.GenerateKbProofOfPossessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>()))
                .ReturnsAsync(KbJwtMock);
            _keyStoreMock.Setup(j =>
                    j.GenerateDPopProofOfPossessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>()))
                .ReturnsAsync(dPopJwtMock);
            
            SetupHttpClientSequence(
                _authServerMetadataResponseWithDPopSupport, 
                _dPopBadRequestTokenResponse,
                _tokenResponseWithDpopSupport, 
                _credentialResponse);
            
            var expectedCredentialResponse = JsonConvert.DeserializeObject<OidCredentialResponse>(CredentialResponse);
            
            //Act
            var actualCredentialResponse = 
                await _oid4VciClientService.RequestCredentialAsync(
                    _oidIssuerMetadata.CredentialsSupported.First().Value,
                    _oidIssuerMetadata,
                    PreAuthorizedCode,
                    TransactionCode
                );
            
            //Assert
            actualCredentialResponse.Item1.Should().BeEquivalentTo(expectedCredentialResponse);
            actualCredentialResponse.Item2.Should().BeEquivalentTo(KeyBindingJwtKeyId);
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

            _oid4VciClientService =
                new Oid4VciClientService(_httpClientFactoryMock.Object, _keyStoreMock.Object);
        }
        
        private void SetupKeyStoreGenerateKeySequence(params string[] responses)
        {
            var responseQueue = new Queue<string>(responses);
            
            _keyStoreMock.Setup(j => j.GenerateKey(It.IsAny<string>()))
                .ReturnsAsync(() => responseQueue.Dequeue());
        }
    }
}
