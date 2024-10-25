using System.Net;
using FluentAssertions;
using Hyperledger.Aries.Agents;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Services;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Services;

public class Oid4VpHaipClientTests 
{
    private const string AuthRequestByReferenceWithRequestUri =
        "openid4vp://?client_id=https%3A%2F%2Fnc-sd-jwt.lambda.d3f.me%2Findex.php%2Fapps%2Fssi_login%2Foidc%2Fcallback&request_uri=https%3A%2F%2Fnc-sd-jwt.lambda.d3f.me%2Findex.php%2Fapps%2Fssi_login%2Foidc%2Frequestobject%2F4ba20ad0cb08545830aa549ab4305c03";

    private const string RequestUriResponse =
        "eyJhbGciOiJFUzI1NiJ9.eyJyZXNwb25zZV90eXBlIjoidnBfdG9rZW4iLCJyZXNwb25zZV9tb2RlIjoiZGlyZWN0X3Bvc3QiLCJjbGllbnRfaWRfc2NoZW1lIjoicmVkaXJlY3RfdXJpIiwiY2xpZW50X2lkIjoiaHR0cHM6Ly92ZXJpZmllci5jb20vcHJlc2VudGF0aW9uL2F1dGhvcml6YXRpb24tcmVzcG9uc2UiLCJjbGllbnRfbWV0YWRhdGFfdXJpIjoiaHR0cHM6Ly92ZXJpZmllci5jb20vbWV0YWRhdGEvMTIzNCIsInJlc3BvbnNlX3VyaSI6Imh0dHBzOi8vdmVyaWZpZXIuY29tL3ByZXNlbnRhdGlvbi9hdXRob3JpemF0aW9uLXJlc3BvbnNlIiwibm9uY2UiOiI4NzU1NDc4NDI2MDI4MDI4MDQ0MjA5MjE4NDE3MTI3NDEzMjQ1OCIsInByZXNlbnRhdGlvbl9kZWZpbml0aW9uIjp7ImlkIjoiNGRkMWMyNmEtMmY0Ni00M2FlLWE3MTEtNzA4ODhjOTNmYjRmIiwiaW5wdXRfZGVzY3JpcHRvcnMiOlt7ImlkIjoiTmV4dGNsb3VkQ3JlZGVudGlhbCIsImZvcm1hdCI6eyJ2YytzZC1qd3QiOnsicHJvb2ZfdHlwZSI6WyJKc29uV2ViU2lnbmF0dXJlMjAyMCJdfX0sImNvbnN0cmFpbnRzIjp7ImxpbWl0X2Rpc2Nsb3N1cmUiOiJyZXF1aXJlZCIsImZpZWxkcyI6W3sicGF0aCI6WyIkLnR5cGUiXSwiZmlsdGVyIjp7InR5cGUiOiJzdHJpbmciLCJjb25zdCI6IlZlcmlmaWVkRU1haWwifX0seyJwYXRoIjpbIiQuY3JlZGVudGlhbFN1YmplY3QuZW1haWwiXX1dfX1dfX0.";

    private const string AuthRequestByValueWithPresentationDefinitionUri =
        "openid4vp:///?client_id=https%3A%2F%2Fsome.de%2Fissuer%2Fdirect_post_vci&response_type=vp_token&response_mode=direct_post&response_uri=https%3A%2F%2Fsome.de%2Fissuer%2Fdirect_post_vci&presentation_definition_uri=https%3A%2F%2Fsome.de%2Fissuer%2Fpresentation-definition&client_id_scheme=redirect_uri&client_metadata_uri=https%3A%2F%2Fsome.de%2Fissuer%2Fclient-metadata&nonce=n0S6_WzA2Mj&state=af0ifjsldkj";

    // private const string UnsignedRequestUriResponse =
    //     "{\"response_type\":\"vp_token\",\"client_id_scheme\":\"redirect_uri\",\"presentation_definition\":{\"id\":\"4dd1c26a-2f46-43ae-a711-70888c93fb4f\",\"input_descriptors\":[{\"id\":\"NextcloudCredential\",\"format\":{\"vc+sd-jwt\":{\"proof_type\":[\"JsonWebSignature2020\"]}},\"constraints\":{\"limit_disclosure\":\"required\",\"fields\":[{\"path\":[\"$.type\"],\"filter\":{\"type\":\"string\",\"const\":\"VerifiedEMail\"}},{\"path\":[\"$.credentialSubject.email\"]}]}}]},\"client_id\":\"https://verifier.com/presentation/authorization-response\",\"nonce\":\"random_nonce_value\",\"response_uri\":\"https://verifier.com/presentation/authorization-response\",\"response_mode\":\"direct_post\",\"client_metadata_uri\":\"https://example.com/client_metadata\",\"scope\":\"openid\",\"state\":\"random_state_value\"}";
    
    private static string PresentationDefinitionUriResponse =>
        new JObject
            {
                ["id"] = "4dd1c26a-2f46-43ae-a711-70888c93fb4f",
                ["input_descriptors"] = new JArray()
                {
                    new JObject()
                    {
                        ["id"] = "NextcloudCredential",
                        ["format"] = new JObject()
                        {
                            ["vc+sd-jwt"] = new JObject()
                            {
                                ["proof_type"] = new JArray("JsonWebSignature2020")
                            }
                        },
                        ["constraints"] = new JObject()
                        {
                            ["limit_disclosure"] = "required",
                            ["fields"] = new JArray()
                            {
                                new JObject()
                                {
                                    ["path"] = new JArray("$.type"),
                                    ["filter"] = new JObject()
                                    {
                                        ["type"] = "string",
                                        ["const"] = "VerifiedEMail"
                                    }
                                },
                                new JObject()
                                {
                                    ["path"] = new JArray("$.credentialSubject.email")
                                }
                            }
                        }
                    }
                }
            }
            .ToString();
    
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
        
    private readonly Mock<IAgentProvider> _agentProviderMock = new();
    private readonly Mock<IMdocStorage> _mdocStorageMock = new();
    private readonly Mock<ISdJwtVcHolderService> _sdJwtVcHolderService = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();

    private Oid4VpHaipClient _oid4VpHaipClient;

    [Fact]
    public async Task CanProcessAuthorizationRequestByReference()
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
            
        _oid4VpHaipClient = new Oid4VpHaipClient(
            new AuthorizationRequestService(_httpClientFactoryMock.Object),
            new PexService(_agentProviderMock.Object, _mdocStorageMock.Object, _sdJwtVcHolderService.Object)
        );

        // Act
        var authorizationRequest = await _oid4VpHaipClient.ProcessAuthorizationRequestAsync(
            AuthorizationRequestUri.FromUri(new Uri(AuthRequestByReferenceWithRequestUri))
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
    
    [Fact]
    public async Task CanProcessAuthorizationRequestByValueWithPresentationDefinitionUri()
    {
        // Arrange
        var presentationDefinitionUriResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(PresentationDefinitionUriResponse)
        };
            
        var verifierMetadataResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(VerifierMetadataResponse)
        };
            
        SetupHttpClientSequence(presentationDefinitionUriResponse, verifierMetadataResponseMessage);
        
        _oid4VpHaipClient = new Oid4VpHaipClient(
            new AuthorizationRequestService(_httpClientFactoryMock.Object),
            new PexService(_agentProviderMock.Object, _mdocStorageMock.Object, _sdJwtVcHolderService.Object)
        );

        // Act
        var authorizationRequest = await _oid4VpHaipClient.ProcessAuthorizationRequestAsync(
            AuthorizationRequestUri.FromUri(new Uri(AuthRequestByValueWithPresentationDefinitionUri))
        );

        // Assert
        authorizationRequest.ClientId.Should().Be("https://some.de/issuer/direct_post_vci");
        authorizationRequest.ResponseUri.Should().Be("https://some.de/issuer/direct_post_vci");
        authorizationRequest.ResponseMode.Should().Be("direct_post");
        authorizationRequest.State.Should().Be("af0ifjsldkj");
        authorizationRequest.Nonce.Should().Be("n0S6_WzA2Mj");
        authorizationRequest.PresentationDefinition.Id.Should().Be("4dd1c26a-2f46-43ae-a711-70888c93fb4f");

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
