using System.Text;
using Hyperledger.Aries.Extensions;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization;
using WalletFramework.Oid4Vc.Oid4Vci.Models.CredentialRequest;
using WalletFramework.Oid4Vc.Oid4Vci.Models.CredentialResponse;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Credential;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer;
using WalletFramework.SdJwtVc.KeyStore.Services;
using static Newtonsoft.Json.JsonConvert;

namespace WalletFramework.Oid4Vc.Oid4Vci.Services.Oid4VciClientService
{
    /// <inheritdoc />
    public class Oid4VciClientService : IOid4VciClientService
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Oid4VciClientService" /> class.
        /// </summary>
        /// <param name="httpClientFactory">
        ///     The factory to create instances of <see cref="HttpClient" />. Used for making HTTP
        ///     requests.
        /// </param>
        /// <param name="keyStore">The key store.</param>
        public Oid4VciClientService(
            IHttpClientFactory httpClientFactory,
            IKeyStore keyStore)
        {
            _httpClientFactory = httpClientFactory;
            _keyStore = keyStore;
        }

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IKeyStore _keyStore;

        /// <inheritdoc />
        public async Task<OidIssuerMetadata> FetchIssuerMetadataAsync(Uri endpoint)
        {
            var baseEndpoint = endpoint
                .AbsolutePath
                .EndsWith("/")
                ? endpoint
                : new Uri(endpoint.OriginalString + "/");

            var metadataUrl = new Uri(baseEndpoint, ".well-known/openid-credential-issuer");

            var response = await _httpClientFactory
                .CreateClient()
                .GetAsync(metadataUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Failed to get Issuer metadata. Status code is {response.StatusCode}"
                );
            }

            return DeserializeObject<OidIssuerMetadata>(
                await response.Content.ReadAsStringAsync()
            ) ?? throw new InvalidOperationException("Failed to deserialize the issuer metadata.");
        }

        /// <inheritdoc />
        public async Task<(OidCredentialResponse, string)> RequestCredentialAsync(
            OidCredentialMetadata credentialMetadata,
            OidIssuerMetadata issuerMetadata,
            TokenResponse tokenResponse)
        {
            var keyId = await _keyStore.GenerateKey();

            var proofOfPossession = await _keyStore.GenerateProofOfPossessionAsync(
                keyId,
                issuerMetadata.CredentialIssuer,
                tokenResponse.CNonce,
                "openid4vci-proof+jwt"
            );

            var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Remove("Authorization");

            httpClient.DefaultRequestHeaders.Add(
                "Authorization",
                $"{tokenResponse.TokenType} {tokenResponse.AccessToken}"
            );

            var responseData = await httpClient.PostAsync(
                issuerMetadata.CredentialEndpoint,
                new StringContent(
                    content: new OidCredentialRequest
                    {
                        Format = credentialMetadata.Format,
                        CredentialDefinition = credentialMetadata.CredentialDefinition,
                        Proof = new OidProofOfPossession
                        {
                            ProofType = "jwt",
                            Jwt = proofOfPossession
                        }
                    }.ToJson(),
                    encoding: Encoding.UTF8,
                    mediaType: "application/json"
                )
            );

            if (!responseData.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Failed to request Credential. Status Code is {responseData.StatusCode}"
                );
            }

            var credentialResponse = DeserializeObject<OidCredentialResponse>(
                await responseData.Content.ReadAsStringAsync()
            );

            if (credentialResponse?.Credential == null)
            {
                throw new InvalidOperationException("Credential response is null.");
            }

            // Todo: Store the credential response in the wallet.
            
            return (credentialResponse, keyId);
        }

        /// <inheritdoc />
        public async Task<TokenResponse> RequestTokenAsync(
            OidIssuerMetadata metadata,
            string preAuthorizedCode,
            string? transactionCode = null)
        {
            var credentialIssuerUrl = new Uri(metadata.CredentialIssuer);

            var getAuthServerUrl =
                !string.IsNullOrEmpty(metadata.AuthorizationServer)
                    ? metadata.AuthorizationServer
                    : string.IsNullOrEmpty(credentialIssuerUrl.AbsolutePath) || credentialIssuerUrl.AbsolutePath == "/"
                        ? $"{credentialIssuerUrl.GetLeftPart(UriPartial.Authority)}/.well-known/oauth-authorization-server"
                        : $"{credentialIssuerUrl.GetLeftPart(UriPartial.Authority)}/.well-known/oauth-authorization-server"
                          + credentialIssuerUrl.AbsolutePath.TrimEnd('/');

            var httpClient = _httpClientFactory.CreateClient();

            var getAuthServerResponse = await httpClient.GetAsync(getAuthServerUrl);

            if (!getAuthServerResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Failed to get authorization server metadata. Status Code is: {getAuthServerResponse.StatusCode}"
                );
            }

            var authServer =
                DeserializeObject<AuthorizationServerMetadata>(
                    await getAuthServerResponse.Content.ReadAsStringAsync()
                )
                ?? throw new InvalidOperationException(
                    "Failed to deserialize the authorization server metadata."
                );

            var response = await httpClient.PostAsync(
                authServer.TokenEndpoint,
                new TokenRequest
                {
                    GrantType = "urn:ietf:params:oauth:grant-type:pre-authorized_code",
                    PreAuthorizedCode = preAuthorizedCode,
                    TransactionCode = transactionCode
                }.ToFormUrlEncoded()
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Failed to get token. Status Code is {response.StatusCode}"
                );
            }

            return DeserializeObject<TokenResponse>(
                await response.Content.ReadAsStringAsync()
            ) ?? throw new InvalidOperationException("Failed to deserialize the token response");
        }
        
        /// <summary>
        ///     Creates a dictionary of the issuer name in different languages based on the issuer metadata.
        /// </summary>
        /// <param name="issuerMetadata">The issuer metadata.</param>
        /// <returns>The dictionary of the issuer name in different languages.</returns>
        private static Dictionary<string, string>? CreateIssuerNameDictionary(OidIssuerMetadata issuerMetadata)
        {
            var issuerNameDictionary = new Dictionary<string, string>();

            foreach (var display in issuerMetadata.Display?.Where(d => d.Locale != null) ??
                                    Enumerable.Empty<OidIssuerDisplay>())
            {
                issuerNameDictionary[display.Locale!] = display.Name!;
            }

            return issuerNameDictionary.Count > 0 ? issuerNameDictionary : null;
        }
    }
}
