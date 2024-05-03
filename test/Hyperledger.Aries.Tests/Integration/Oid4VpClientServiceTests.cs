using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hyperledger.Aries.Storage;
using Hyperledger.TestHarness.Mock;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SD_JWT;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Credential;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Credential.Attributes;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Services;
using WalletFramework.SdJwtVc.KeyStore.Services;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;
using Xunit;

namespace Hyperledger.Aries.Tests.Integration
{
    public class Oid4VpClientServiceTests : IAsyncLifetime
    {
        private const string AuthRequestWithRequestUri =
            "haip://?client_id=https%3A%2F%2Fnc-sd-jwt.lambda.d3f.me%2Findex.php%2Fapps%2Fssi_login%2Foidc%2Fcallback&request_uri=https%3A%2F%2Fnc-sd-jwt.lambda.d3f.me%2Findex.php%2Fapps%2Fssi_login%2Foidc%2Frequestobject%2F4ba20ad0cb08545830aa549ab4305c03";

        private const string CombinedIssuance =
            "eyJ4NWMiOlsiTUlJQ09qQ0NBZUdnQXdJQkFnSUJBekFLQmdncWhrak9QUVFEQWpCak1Rc3dDUVlEVlFRR0V3SkVSVEVQTUEwR0ExVUVCd3dHUW1WeWJHbHVNUjB3R3dZRFZRUUtEQlJDZFc1a1pYTmtjblZqYTJWeVpXa2dSMjFpU0RFS01BZ0dBMVVFQ3d3QlNURVlNQllHQTFVRUF3d1BTVVIxYm1sdmJpQlVaWE4wSUVOQk1CNFhEVEl6TURjeE9ERXlOVE16TlZvWERUSTRNRGN4TmpFeU5UTXpOVm93V1RFTE1Ba0dBMVVFQmhNQ1JFVXhIVEFiQmdOVkJBb01GRUoxYm1SbGMyUnlkV05yWlhKbGFTQkhiV0pJTVFvd0NBWURWUVFMREFGSk1SOHdIUVlEVlFRRERCWldaWEpwWm1sbFpDQkZMVTFoYVd3Z1NYTnpkV1Z5TUZrd0V3WUhLb1pJemowQ0FRWUlLb1pJemowREFRY0RRZ0FFOGoxOEsyZTRjZGRkdjRzaGRFUE84Z251MTJnM242RDFtRC9KU09TcEdDZlc5YUdoaU92bHpPck5icGRzTGVlWjVtclV3SXpra3BrMUhPVnZwSTNwVXFPQmp6Q0JqREFkQmdOVkhRNEVGZ1FVTFo4eWFCbDJJUVJWeCtrTGY4d3ZmRFpIY1pRd0RBWURWUjBUQVFIL0JBSXdBREFPQmdOVkhROEJBZjhFQkFNQ0I0QXdMQVlEVlIwUkJDVXdJNEloYVhOemRXVnlMVzl3Wlc1cFpEUjJZeTV6YzJrdWRHbHlMbUoxWkhKMUxtUmxNQjhHQTFVZEl3UVlNQmFBRkUrVzZ6N2FqVHVtZXgrWWNGYm9OclZlQzJ0Uk1Bb0dDQ3FHU000OUJBTUNBMGNBTUVRQ0lDZU5KYi85OENkV3RPdEtrREs0bm1WSGV4N0ZJclJQMlBRY3lmOVIzUGdPQWlCUHNkeENsakZXcTdxUGFOdUthUzhnTjRqZEkyVXUrNlNKaWZLZGp6SDdsQT09IiwiTUlJQ0xUQ0NBZFNnQXdJQkFnSVVNWVVIaEdEOWhVL2MwRW82bVc4cmpqZUordDB3Q2dZSUtvWkl6ajBFQXdJd1l6RUxNQWtHQTFVRUJoTUNSRVV4RHpBTkJnTlZCQWNNQmtKbGNteHBiakVkTUJzR0ExVUVDZ3dVUW5WdVpHVnpaSEoxWTJ0bGNtVnBJRWR0WWtneENqQUlCZ05WQkFzTUFVa3hHREFXQmdOVkJBTU1EMGxFZFc1cGIyNGdWR1Z6ZENCRFFUQWVGdzB5TXpBM01UTXdPVEkxTWpoYUZ3MHpNekEzTVRBd09USTFNamhhTUdNeEN6QUpCZ05WQkFZVEFrUkZNUTh3RFFZRFZRUUhEQVpDWlhKc2FXNHhIVEFiQmdOVkJBb01GRUoxYm1SbGMyUnlkV05yWlhKbGFTQkhiV0pJTVFvd0NBWURWUVFMREFGSk1SZ3dGZ1lEVlFRRERBOUpSSFZ1YVc5dUlGUmxjM1FnUTBFd1dUQVRCZ2NxaGtqT1BRSUJCZ2dxaGtqT1BRTUJCd05DQUFTRUh6OFlqckZ5VE5IR0x2TzE0RUF4bTl5aDhiS09na1V6WVdjQzFjdnJKbjVKZ0hZSE14WmJOTU8xM0VoMEVyMjczOFFRT2dlUm9aTUlUYW9ka2ZOU28yWXdaREFkQmdOVkhRNEVGZ1FVVDViclB0cU5PNlo3SDVod1Z1ZzJ0VjRMYTFFd0h3WURWUjBqQkJnd0ZvQVVUNWJyUHRxTk82WjdINWh3VnVnMnRWNExhMUV3RWdZRFZSMFRBUUgvQkFnd0JnRUIvd0lCQURBT0JnTlZIUThCQWY4RUJBTUNBWVl3Q2dZSUtvWkl6ajBFQXdJRFJ3QXdSQUlnWTBEZXJkQ3h0NHpHUFluOHlOckR4SVdDSkhwenE0QmRqZHNWTjJvMUdSVUNJQjBLQTdiRzFGVkIxSWlLOGQ1N1FBTCtQRzlYNWxkS0c3RWtvQW1oV1ZLZSJdLCJraWQiOiJNR3d3WjZSbE1HTXhDekFKQmdOVkJBWVRBa1JGTVE4d0RRWURWUVFIREFaQ1pYSnNhVzR4SFRBYkJnTlZCQW9NRkVKMWJtUmxjMlJ5ZFdOclpYSmxhU0JIYldKSU1Rb3dDQVlEVlFRTERBRkpNUmd3RmdZRFZRUUREQTlKUkhWdWFXOXVJRlJsYzNRZ1EwRUNBUU09IiwidHlwIjoidmMrc2Qtand0IiwiYWxnIjoiRVMyNTYifQ.eyJfc2QiOlsibmJkd2Z0NE9QdmcycFFlaVFYOHdoc2hnZ0VVTTdBY29mdHRWUE95ejJpdyIsIkxoQjF2dE9WM1ZHd3V6QmhKWnhoUUd5OUNSY0l0dC1QSmkydDRvRk83X28iLCJZNEl2Uk4yY2VDU2V6aXZKRjREMHFDc0JQNW81eUZVdDJiXy1YRkFXTGZjIiwieU9kNkRJbGFDUERXTG9xLUJfY2JQWTY4dFZmV18wU25NRGQzeU5qRDIxRSJdLCJfc2RfYWxnIjoic2hhLTI1NiIsImlzcyI6Imh0dHBzOi8vaXNzdWVyLW9wZW5pZDR2Yy5zc2kudGlyLmJ1ZHJ1LmRlIiwiY25mIjp7Imp3ayI6eyJrdHkiOiJFQyIsImNydiI6IlAtMjU2IiwieCI6IjN1ZVRuN3VYLTZpSmxJYllOU2xrM0NSa3pwVDZGYldNMkxjdDZhdy1HZEEiLCJ5IjoiWlFlZnFJVnlzOG1MT05PZXBSclNsOWhXVGhGai1HTUVWR0pGUVk1TXQ2TSJ9fSwidHlwZSI6IlZlcmlmaWVkRU1haWwiLCJleHAiOjE2OTcyODIwOTgsImlhdCI6MTY5NjQxODA5OH0.Sbj1LaWpz45iqsdS8NFaLFgZ7G5hj1ofYLlO4rTI-jHELD6ORMGe1LVHe7IiOr_DNCDDde0ScGIEZKRiNCHEfA~WyJZTVVoZzh3Q2RHUGJjV245NW9MbUtBIiwiZW1haWwiLCJqb3RlbWVrMzYxQGZlc2dyaWQuY29tIl0";

        private const string ExpectedRedirectUrl =
            "https://client.example.org/cb#response_code=091535f699ea575c7937fa5f0f454aee";

        private const string KeyBindingJwtMock =
            "eyJhbGciOiJFUzI1NiIsInR5cCI6ImtiK2p3dCJ9.eyJhdWQiOiJodHRwczovL3ZlcmlmaWVyLnNzaS50aXIuYnVkcnUuZGUvcHJlc2VudGF0aW9uL2F1dGhvcml6YXRpb24tcmVzcG9uc2UiLCJub25jZSI6IkxIc1lGRnlpMnNXQzZIM3ZSWVFsNFQiLCJpYXQiOjE2OTY0MjcwNzR9.Kxj1e7ucZeAnFnfOjo05QnW-DYeEprciDqkOhe6fhXIWprEYd1NJ6a0gpZJ66oTJsv49ExvDOKTLOzt6R75gcg";

        private const string KeyId = "KeyId";

        private const string RequestUriResponse =
            "eyJ4NWMiOlsiTUlJQ0x6Q0NBZFdnQXdJQkFnSUJCREFLQmdncWhrak9QUVFEQWpCak1Rc3dDUVlEVlFRR0V3SkVSVEVQTUEwR0ExVUVCd3dHUW1WeWJHbHVNUjB3R3dZRFZRUUtEQlJDZFc1a1pYTmtjblZqYTJWeVpXa2dSMjFpU0RFS01BZ0dBMVVFQ3d3QlNURVlNQllHQTFVRUF3d1BTVVIxYm1sdmJpQlVaWE4wSUVOQk1CNFhEVEl6TURnd016QTROREkwTkZvWERUSTRNRGd3TVRBNE5ESTBORm93VlRFTE1Ba0dBMVVFQmhNQ1JFVXhIVEFiQmdOVkJBb01GRUoxYm1SbGMyUnlkV05yWlhKbGFTQkhiV0pJTVFvd0NBWURWUVFMREFGSk1Sc3dHUVlEVlFRRERCSlBjR1Z1U1dRMFZsQWdWbVZ5YVdacFpYSXdXVEFUQmdjcWhrak9QUUlCQmdncWhrak9QUU1CQndOQ0FBUnNoUzVDaVBrSzVXRUN1RHpybmN0SXBwYm1nc1lkOURzT1lEcElFeFpFczFmUWNOeXZrQjVFZU5Xc2MwU0ExUU5xd3dHVzRndUZLZzBJZjFKR0R4VWZvNEdITUlHRU1CMEdBMVVkRGdRV0JCUmZMQVBzeG1Mc3AxblEvRk12RkkzN0MzQmxZREFNQmdOVkhSTUJBZjhFQWpBQU1BNEdBMVVkRHdFQi93UUVBd0lIZ0RBa0JnTlZIUkVFSFRBYmdobDJaWEpwWm1sbGNpNXpjMmt1ZEdseUxtSjFaSEoxTG1SbE1COEdBMVVkSXdRWU1CYUFGRStXNno3YWpUdW1leCtZY0Zib05yVmVDMnRSTUFvR0NDcUdTTTQ5QkFNQ0EwZ0FNRVVDSUNWZURUMnNkZHhySEMrZ0ZJTUVmc3huc0lXRmdIdnZlZnBuWXZrb0RjbHdBaUVBMlFnRVRHV3hIWUVObWxsNDA2VUNwYnFRb1kzMzJPbE9qdDUwWjc2WHBtQT0iLCJNSUlDTFRDQ0FkU2dBd0lCQWdJVU1ZVUhoR0Q5aFUvYzBFbzZtVzhyamplSit0MHdDZ1lJS29aSXpqMEVBd0l3WXpFTE1Ba0dBMVVFQmhNQ1JFVXhEekFOQmdOVkJBY01Ca0psY214cGJqRWRNQnNHQTFVRUNnd1VRblZ1WkdWelpISjFZMnRsY21WcElFZHRZa2d4Q2pBSUJnTlZCQXNNQVVreEdEQVdCZ05WQkFNTUQwbEVkVzVwYjI0Z1ZHVnpkQ0JEUVRBZUZ3MHlNekEzTVRNd09USTFNamhhRncwek16QTNNVEF3T1RJMU1qaGFNR014Q3pBSkJnTlZCQVlUQWtSRk1ROHdEUVlEVlFRSERBWkNaWEpzYVc0eEhUQWJCZ05WQkFvTUZFSjFibVJsYzJSeWRXTnJaWEpsYVNCSGJXSklNUW93Q0FZRFZRUUxEQUZKTVJnd0ZnWURWUVFEREE5SlJIVnVhVzl1SUZSbGMzUWdRMEV3V1RBVEJnY3Foa2pPUFFJQkJnZ3Foa2pPUFFNQkJ3TkNBQVNFSHo4WWpyRnlUTkhHTHZPMTRFQXhtOXloOGJLT2drVXpZV2NDMWN2ckpuNUpnSFlITXhaYk5NTzEzRWgwRXIyNzM4UVFPZ2VSb1pNSVRhb2RrZk5TbzJZd1pEQWRCZ05WSFE0RUZnUVVUNWJyUHRxTk82WjdINWh3VnVnMnRWNExhMUV3SHdZRFZSMGpCQmd3Rm9BVVQ1YnJQdHFOTzZaN0g1aHdWdWcydFY0TGExRXdFZ1lEVlIwVEFRSC9CQWd3QmdFQi93SUJBREFPQmdOVkhROEJBZjhFQkFNQ0FZWXdDZ1lJS29aSXpqMEVBd0lEUndBd1JBSWdZMERlcmRDeHQ0ekdQWW44eU5yRHhJV0NKSHB6cTRCZGpkc1ZOMm8xR1JVQ0lCMEtBN2JHMUZWQjFJaUs4ZDU3UUFMK1BHOVg1bGRLRzdFa29BbWhXVktlIl0sImtpZCI6Ik1Hd3daNlJsTUdNeEN6QUpCZ05WQkFZVEFrUkZNUTh3RFFZRFZRUUhEQVpDWlhKc2FXNHhIVEFiQmdOVkJBb01GRUoxYm1SbGMyUnlkV05yWlhKbGFTQkhiV0pJTVFvd0NBWURWUVFMREFGSk1SZ3dGZ1lEVlFRRERBOUpSSFZ1YVc5dUlGUmxjM1FnUTBFQ0FRUT0iLCJ0eXAiOiJKV1QiLCJhbGciOiJFUzI1NiJ9.eyJyZXNwb25zZV90eXBlIjoidnBfdG9rZW4iLCJwcmVzZW50YXRpb25fZGVmaW5pdGlvbiI6eyJpZCI6IjE1ZDQwNjU0LWM2NTgtNDkzOC1hYzA3LWVjYjQxYzlhZmIxMCIsImlucHV0X2Rlc2NyaXB0b3JzIjpbeyJpZCI6IjUwYjZlNGYzLTYyMmEtNDk3NC1iMzMwLTVlNzIwZWM5MjJiZiIsImZvcm1hdCI6eyJ2YytzZC1qd3QiOnsicHJvb2ZfdHlwZSI6WyJKc29uV2ViU2lnbmF0dXJlMjAyMCJdfX0sImNvbnN0cmFpbnRzIjp7ImxpbWl0X2Rpc2Nsb3N1cmUiOiJyZXF1aXJlZCIsImZpZWxkcyI6W3sicGF0aCI6WyIkLnR5cGUiXSwiZmlsdGVyIjp7InR5cGUiOiJzdHJpbmciLCJjb25zdCI6IlZlcmlmaWVkRU1haWwifX0seyJwYXRoIjpbIiQuZW1haWwiXX1dfX1dfSwicmVzcG9uc2VfdXJpIjoiaHR0cHM6Ly92ZXJpZmllci5zc2kudGlyLmJ1ZHJ1LmRlL3ByZXNlbnRhdGlvbi9hdXRob3JpemF0aW9uLXJlc3BvbnNlIiwibm9uY2UiOiJZZjg4dGRlZzhZTTkyM3E0aFFBRzlPIiwiY2xpZW50X2lkIjoiaHR0cHM6Ly92ZXJpZmllci5zc2kudGlyLmJ1ZHJ1LmRlL3ByZXNlbnRhdGlvbi9hdXRob3JpemF0aW9uLXJlc3BvbnNlIiwicmVzcG9uc2VfbW9kZSI6ImRpcmVjdF9wb3N0In0.sdeLcG6Ta4ozfbDuHBr2Vq-Ro2WpdUIhJWy3BgazyvrgkQw27uTFGioPWXNCruK5H5E5nvHS420u5tv0671tjg";

        private const string Vct = "VerifiedEmail";


        public Oid4VpClientServiceTests()
        {
            var holder = new Holder();
            var walletRecordService = new DefaultWalletRecordService();
            var pexService = new PexService();
            _sdJwtVcHolderService = new DefaultSdJwtVcHolderService(holder, _keyStoreMock.Object, walletRecordService);
            _oid4VpHaipClient = new Oid4VpHaipClient(_httpClientFactoryMock.Object, pexService);
            _oid4VpRecordService = new Oid4VpRecordService(walletRecordService);

            _oid4VpClientService = new Oid4VpClientService(
                _httpClientFactoryMock.Object,
                _sdJwtVcHolderService,
                _oid4VpHaipClient,
                _loggerMock.Object,
                _oid4VpRecordService
            );

            _keyStoreMock.Setup(keyStore =>
                    keyStore.GenerateKbProofOfPossessionAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()
                    )
                )
                .ReturnsAsync(KeyBindingJwtMock);
        }

        private readonly DefaultSdJwtVcHolderService _sdJwtVcHolderService;

        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        private readonly Mock<ILogger<Oid4VpClientService>> _loggerMock = new Mock<ILogger<Oid4VpClientService>>();
        private readonly Mock<IKeyStore> _keyStoreMock = new Mock<IKeyStore>();
        private MockAgent? _agent1;
        private readonly MockAgentRouter _router = new MockAgentRouter();

        private readonly Oid4VpClientService _oid4VpClientService;
        private readonly Oid4VpHaipClient _oid4VpHaipClient;
        private readonly Oid4VpRecordService _oid4VpRecordService;

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

        private readonly WalletConfiguration _config1 = new WalletConfiguration() { Id = Guid.NewGuid().ToString() };
        private readonly WalletCredentials _cred = new WalletCredentials() { Key = "2" };

        [Fact]
        public async Task CanExecuteOpenId4VpFlow()
        {
            //Arrange
            SetupHttpClient(RequestUriResponse);
            
            await _sdJwtVcHolderService.StoreAsync(_agent1.Context, CombinedIssuance, KeyId, _oidIssuerMetadata.CredentialIssuer, null, null, null, null);

            //Act
            var (authorizationRequest, credentials) =
                await _oid4VpClientService.ProcessAuthorizationRequestAsync(
                    _agent1.Context,
                    new Uri(AuthRequestWithRequestUri)
                );

            var selectedCandidates = new SelectedCredential
            {
                InputDescriptorId = credentials.First().InputDescriptorId,
                Credential = credentials.First().Credentials.First()
            };

            SetupHttpClient(
                "{'redirect_uri':'https://client.example.org/cb#response_code=091535f699ea575c7937fa5f0f454aee'}"
            );

            var response = await _oid4VpClientService.SendAuthorizationResponseAsync(
                _agent1.Context,
                authorizationRequest,
                new[] { selectedCandidates }
            );

            // Assert
            credentials.Length.Should().Be(1);
            
            response.Should().BeEquivalentTo(new Uri(ExpectedRedirectUrl));
            
            (await _oid4VpRecordService.ListAsync(_agent1.Context)).Count.Should().Be(1);
        }

        public async Task DisposeAsync()
        {
            await _agent1.Dispose();
        }

        public async Task InitializeAsync()
        {
            _agent1 = 
                await MockUtils.CreateAsync(
                    "agent1",
                    _config1,
                    _cred,
                    new MockAgentHttpHandler(
                        cb => _router.RouteMessage(cb.name, cb.data)
                    )
                );
            
            _router.RegisterAgent(_agent1);
        }

        private void SetupHttpClient(string response)
        {
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response)
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => httpResponseMessage);

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        }
    }
}
