using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hyperledger.Aries.Features.OpenId4Vc.KeyStore.Services;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Authorization;
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
        private const string AuthServerMetadata =
            "{\"issuer\":\"https://issuer.io\",\"token_endpoint\":\"https://issuer.io/token\",\"token_endpoint_auth_methods_supported\":[\"urn:ietf:params:oauth:client-assertion-type:verifiable-presentation\"],\"response_types_supported\":[\"urn:ietf:params:oauth:grant-type:pre-authorized_code\"]}\n";

        private const string DPoPAuthServerMetadata =
            "{\"issuer\":\"https://issuer.io\",\"token_endpoint\":\"https://issuer.io/token\",\"token_endpoint_auth_methods_supported\":[\"urn:ietf:params:oauth:client-assertion-type:verifiable-presentation\"],\"response_types_supported\":[\"urn:ietf:params:oauth:grant-type:pre-authorized_code\"],\"dpop_signing_alg_values_supported\":[\"ES256\"]}\n";
        
        private const string IssuerMetadataResponseContent =
            "{\"credential_issuer\":\"https://issuer.io/\",\"credential_endpoint\":\"https://issuer.io/credential\",\"display\":[{\"name\":\"Aussteller\",\"locale\":\"de-DE\"},{\"name\":\"Issuer\",\"locale\":\"en-US\"}],\"credentials_supported\":{\"IdentityCredential\":{\"format\":\"vc+sd-jwt\",\"scope\":\"IdentityCredential_SD-JWT-VC\",\"cryptographic_binding_methods_supported\":[\"did:example\"],\"cryptographic_suites_supported\":[\"ES256K\"],\"display\":[{\"name\":\"IdentityCredential\",\"locale\":\"en-US\",\"background_color\":\"#12107c\",\"text_color\":\"#FFFFFF\"}],\"credential_definition\":{\"type\":\"IdentityCredential\",\"claims\":{\"given_name\":{\"display\":[{\"name\":\"GivenName\",\"locale\":\"en-US\"},{\"name\":\"Vorname\",\"locale\":\"de-DE\"}]},\"last_name\":{\"display\":[{\"name\":\"Surname\",\"locale\":\"en-US\"},{\"name\":\"Nachname\",\"locale\":\"de-DE\"}]},\"email\":{},\"phone_number\":{},\"address\":{\"street_address\":{},\"locality\":{},\"region\":{},\"country\":{}},\"birthdate\":{},\"is_over_18\":{},\"is_over_21\":{},\"is_over_65\":{}}}}}}";

        private const string PreAuthorizedCode = "1234";

        private const string TokenResponse =
            "{\"access_token\":\"eyJhbGciOiJSUzI1NiIsInR5cCI6Ikp..sHQ\",\"token_type\":\"bearer\",\"expires_in\": 86400,\"c_nonce\": \"tZignsnFbp\",\"c_nonce_expires_in\":86400}";

        private const string DPopBadRequestTokenResponse = "{\"error\":\"use_dpop_nonce\"}";
        
        private const string DPopSuccessTokenResponse =
            "{\"access_token\":\"eyJhbGciOiJSUzI1NiIsInR5cCI6Ikp..sHQ\",\"token_type\":\"bearer\",\"expires_in\": 86400,\"c_nonce\": \"tZignsnFbp\",\"c_nonce_expires_in\":86400}";
        
        private const string Vct = "VerifiedEmail";

        private const string DPopNonce = "someRadnomNonceStringFromIssuer";

        private readonly HttpResponseMessage _authServerMetadataResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(AuthServerMetadata)
        };
        
        private readonly HttpResponseMessage _dPopAuthServerMetadataResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(DPoPAuthServerMetadata)
        };

        private readonly HttpResponseMessage _issuerMetadataResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(IssuerMetadataResponseContent)
        };

        private readonly HttpResponseMessage _tokenResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(TokenResponse)
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
        public async Task AuthServerUriIsBuiltCorrectly()
        {
            // Arrange
            const string authServerUri = "https://authserver.io";
            _oidIssuerMetadata.AuthorizationServer = authServerUri;

            SetupHttpClientSequence(_authServerMetadataResponse, _tokenResponse);

            var expectedUri = new Uri(authServerUri);

            // Act
            await _oid4VciClientService.RequestTokenAsync(_oidIssuerMetadata, PreAuthorizedCode);

            // Assert
            _httpMessageHandlerMock.Protected()
                .Verify("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get && req.RequestUri == expectedUri),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData("https://issuer.io", "https://issuer.io/.well-known/oauth-authorization-server")]
        [InlineData("https://issuer.io/", "https://issuer.io/.well-known/oauth-authorization-server")]
        [InlineData("https://issuer.io/issuer1", "https://issuer.io/.well-known/oauth-authorization-server/issuer1")]
        [InlineData("https://issuer.io/issuer1/", "https://issuer.io/.well-known/oauth-authorization-server/issuer1")]
        public async Task AuthServerUriIsBuiltFromCredentialIssuerCorrectly(string issuer, string expectedUriString)
        {
            // Arrange
            _oidIssuerMetadata.CredentialIssuer = issuer;

            SetupHttpClientSequence(_authServerMetadataResponse, _tokenResponse);

            var expectedUri = new Uri(expectedUriString);

            // Act
            await _oid4VciClientService.RequestTokenAsync(_oidIssuerMetadata, PreAuthorizedCode);

            // Assert
            _httpMessageHandlerMock.Protected()
                .Verify("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get && req.RequestUri == expectedUri),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task CanGetIssuerMetadataAsync()
        {
            // Arrange
            SetupHttpClientSequence(_issuerMetadataResponse);

            var expectedMetadata = JsonConvert.DeserializeObject<OidIssuerMetadata>(IssuerMetadataResponseContent);

            // Act
            var actualMetadata = await _oid4VciClientService.FetchIssuerMetadataAsync(new Uri("https://issuer.io"));

            // Assert
            actualMetadata.Should().BeEquivalentTo(expectedMetadata, options =>
            {
                options.AllowingInfiniteRecursion();
                return options;
            });
        }

        [Fact]
        public async Task CanRequestCredentialAsync()
        {
            // Arrange
            const string jwtMock = "mockJwt";
            const string keyId = "keyId";

            _keyStoreMock.Setup(j => j.GenerateKey(It.IsAny<string>()))
                .ReturnsAsync(keyId);
            _keyStoreMock.Setup(j =>
                    j.GenerateProofOfPossessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>()))
                .ReturnsAsync(jwtMock);

            const string credentialResponse =
                "{\"format\":\"vc+sd-jwt\",\"credential\":\"eyJhbGciOiAiRVMyNTYifQ.eyJfc2QiOlsiT0dfT2lCMk5ZS0JzTVhIOFVVb2luREhUT1h5VER1Z3JPdE94RFI2NF9ZcyIsIlQzbHRYQUFtODNJTXRUYkRTb1J2d1g2Tk10em1scV9ZWG9Vd1EwZDY0NEUiXSwiaXNzIjoiaHR0cHM6Ly9pc3N1ZXIuaW8vIiwiaWF0IjoxNTE2MjM5MDIyLCJ0eXBlIjoiVmVyaWZpZWRFbWFpbCIsImV4cCI6MTUxNjI0NzAyMiwiX3NkX2FsZyI6InNoYS0yNTYiLCJjbmYiOnsiandrIjp7Imt0eSI6IkVDIiwiY3J2IjoiUC0yNTYiLCJ4IjoiVENBRVIxOVp2dTNPSEY0ajRXNHZmU1ZvSElQMUlMaWxEbHM3dkNlR2VtYyIsInkiOiJaeGppV1diWk1RR0hWV0tWUTRoYlNJaXJzVmZ1ZWNDRTZ0NGpUOUYySFpRIn19LCJhbGciOiJFUzI1NiJ9.OVSoCqHZLgAPaYK27gJx6J1ejwskP62xIHryqc1ZJYOR8yZdicSF4KXBk5qgocWZdiqEsri5Q3sW69xIfbmXSA~WyJseVMxN1ZzenNGb3doaFBnY3VuOTFRIiwgImV4cCIsIDE1NDE0OTQ3MjRd~WyJaRmNwSWxTNlJ5eWV2U3JTeFdJbDZRIiwgImdpdmVuX25hbWUiLCAiSm9obiJd~WyJVSHVVVUNlOWZzNUdody1mZ0JJWi13IiwgImZhbWlseV9uYW1lIiwgIkRvZSJd~WyJ3ZnR5YkpzYktzVWJDay1XaWpaQ3RRIiwgImVtYWlsIiwgInRlc3RAZXhhbXBsZS5jb20iXQ\"}";
            SetupHttpClientSequence(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(credentialResponse)
                });

            var expectedCredentialResponse = JsonConvert.DeserializeObject<OidCredentialResponse>(credentialResponse);

            var mockTokenResponse = new TokenResponse
            {
                AccessToken = "sampleAccessToken",
                TokenType = "bearer",
                ExpiresIn = 3600,
                CNonce = "sampleCNonce",
                CNonceExpiresIn = 3600
            };

            // Act
            var actualCredentialResponse = await _oid4VciClientService.RequestCredentialAsync(
                _oidIssuerMetadata.CredentialsSupported.First().Value,
                _oidIssuerMetadata,
                mockTokenResponse,
                null,
                null
            );

            // Assert
            actualCredentialResponse.Item1.Should().BeEquivalentTo(expectedCredentialResponse);
            actualCredentialResponse.Item2.Should().BeEquivalentTo(keyId);
        }
        
        [Fact]
        public async Task CanRequestCredentialWithDPopAsync()
        {
            // Arrange
            const string jwtMock = "mockJwt";
            const string keyId = "keyId";
            const string dPopKeyId = "keyId";

            _keyStoreMock.Setup(j => j.GenerateKey(It.IsAny<string>()))
                .ReturnsAsync(keyId);
            _keyStoreMock.Setup(j =>
                    j.GenerateDPopProofOfPossessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>()))
                .ReturnsAsync(jwtMock);

            const string credentialResponse =
                "{\"format\":\"vc+sd-jwt\",\"credential\":\"eyJhbGciOiAiRVMyNTYifQ.eyJfc2QiOlsiT0dfT2lCMk5ZS0JzTVhIOFVVb2luREhUT1h5VER1Z3JPdE94RFI2NF9ZcyIsIlQzbHRYQUFtODNJTXRUYkRTb1J2d1g2Tk10em1scV9ZWG9Vd1EwZDY0NEUiXSwiaXNzIjoiaHR0cHM6Ly9pc3N1ZXIuaW8vIiwiaWF0IjoxNTE2MjM5MDIyLCJ0eXBlIjoiVmVyaWZpZWRFbWFpbCIsImV4cCI6MTUxNjI0NzAyMiwiX3NkX2FsZyI6InNoYS0yNTYiLCJjbmYiOnsiandrIjp7Imt0eSI6IkVDIiwiY3J2IjoiUC0yNTYiLCJ4IjoiVENBRVIxOVp2dTNPSEY0ajRXNHZmU1ZvSElQMUlMaWxEbHM3dkNlR2VtYyIsInkiOiJaeGppV1diWk1RR0hWV0tWUTRoYlNJaXJzVmZ1ZWNDRTZ0NGpUOUYySFpRIn19LCJhbGciOiJFUzI1NiJ9.OVSoCqHZLgAPaYK27gJx6J1ejwskP62xIHryqc1ZJYOR8yZdicSF4KXBk5qgocWZdiqEsri5Q3sW69xIfbmXSA~WyJseVMxN1ZzenNGb3doaFBnY3VuOTFRIiwgImV4cCIsIDE1NDE0OTQ3MjRd~WyJaRmNwSWxTNlJ5eWV2U3JTeFdJbDZRIiwgImdpdmVuX25hbWUiLCAiSm9obiJd~WyJVSHVVVUNlOWZzNUdody1mZ0JJWi13IiwgImZhbWlseV9uYW1lIiwgIkRvZSJd~WyJ3ZnR5YkpzYktzVWJDay1XaWpaQ3RRIiwgImVtYWlsIiwgInRlc3RAZXhhbXBsZS5jb20iXQ\"}";
            SetupHttpClientSequence(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(credentialResponse)
                });

            var expectedCredentialResponse = JsonConvert.DeserializeObject<OidCredentialResponse>(credentialResponse);

            var mockTokenResponse = new TokenResponse
            {
                AccessToken = "sampleAccessToken",
                TokenType = "bearer",
                ExpiresIn = 3600,
                CNonce = "sampleCNonce",
                CNonceExpiresIn = 3600
            };

            // Act
            var actualCredentialResponse = await _oid4VciClientService.RequestCredentialAsync(
                _oidIssuerMetadata.CredentialsSupported.First().Value,
                _oidIssuerMetadata,
                mockTokenResponse,
                dPopKeyId,
                DPopNonce
            );

            // Assert
            actualCredentialResponse.Item1.Should().BeEquivalentTo(expectedCredentialResponse);
            actualCredentialResponse.Item2.Should().BeEquivalentTo(keyId);
        }

        [Fact]
        public async Task CanRequestTokenAsync()
        {
            // Arrange
            SetupHttpClientSequence(
                _authServerMetadataResponse,
                _tokenResponse);

            var expectedTokenResponse = JsonConvert.DeserializeObject<TokenResponse>(TokenResponse);

            // Act
            var actualTokenResponse =
                await _oid4VciClientService.RequestTokenAsync(_oidIssuerMetadata, PreAuthorizedCode);

            // Assert
            actualTokenResponse.Item1.Should().BeEquivalentTo(expectedTokenResponse);
            actualTokenResponse.Item2.Should().BeEquivalentTo(null);
            actualTokenResponse.Item3.Should().BeEquivalentTo(null);
        }
        
        [Fact]
        public async Task CanRequestTokenWithDPopAsync()
        {
            // Arrange
            const string jwtMock = "mockJwt";
            const string dPopKeyId = "keyId";
            
            _keyStoreMock.Setup(j => j.GenerateKey(It.IsAny<string>()))
                .ReturnsAsync(dPopKeyId);
            _keyStoreMock.Setup(j =>
                    j.GenerateDPopProofOfPossessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>()))
                .ReturnsAsync(jwtMock);
            
            SetupHttpClientSequence(
                _dPopAuthServerMetadataResponse,
                _dPopBadRequestTokenResponse,
                _tokenResponse);

            var expectedTokenResponse = (JsonConvert.DeserializeObject<TokenResponse>(TokenResponse), dPopKeyId, DPopNonce);

            // Act
            var actualTokenResponse =
                await _oid4VciClientService.RequestTokenAsync(_oidIssuerMetadata, PreAuthorizedCode);

            // Assert
            actualTokenResponse.Should().BeEquivalentTo(expectedTokenResponse);
        }

        [Theory]
        [InlineData("https://issuer.io", "https://issuer.io/.well-known/openid-credential-issuer")]
        [InlineData("https://issuer.io/", "https://issuer.io/.well-known/openid-credential-issuer")]
        [InlineData("https://issuer.io/issuer1", "https://issuer.io/issuer1/.well-known/openid-credential-issuer")]
        [InlineData("https://issuer.io/issuer1/", "https://issuer.io/issuer1/.well-known/openid-credential-issuer")]
        public async Task CredentialIssuerUriIsBuiltCorrectly(string inputUri, string expectedUriString)
        {
            // Arrange
            SetupHttpClientSequence(_issuerMetadataResponse);

            // Act
            var expectedUri = new Uri(expectedUriString);
            await _oid4VciClientService.FetchIssuerMetadataAsync(new Uri(inputUri));

            // Assert
            _httpMessageHandlerMock.Protected()
                .Verify("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get && req.RequestUri == expectedUri),
                    ItExpr.IsAny<CancellationToken>());
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
    }
}
