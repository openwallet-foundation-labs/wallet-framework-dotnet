using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Hyperledger.Aries.Extensions;
using Hyperledger.Aries.Features.OpenId4Vc.KeyStore.Services;
using Hyperledger.Aries.Features.OpenID4VC.VCI.Extensions;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Authorization;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialRequest;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialResponse;
using Hyperledger.Aries.Features.OpenID4VC.VCI.Models.DPop;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Credential;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Issuer;
using Newtonsoft.Json.Linq;
using static Newtonsoft.Json.JsonConvert;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vci.Services.Oid4VciClientService
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
        public async Task<(OidCredentialResponse credentialResponse, string keyId)> RequestCredentialAsync(
            OidCredentialMetadata credentialMetadata,
            OidIssuerMetadata issuerMetadata,
            string preAuthorizedCode,
            string? transactionCode)
        {
            var authorizationServerMetadata = await FetchAuthorizationServerMetadataAsync(issuerMetadata);

            var oAuthToken = authorizationServerMetadata.IsDPoPSupported
                ? await RequestTokenWithDPop(authorizationServerMetadata, preAuthorizedCode, transactionCode)
                : await RequestTokenWithoutDPop(authorizationServerMetadata, preAuthorizedCode, transactionCode);
            
            return oAuthToken.IsDPoPRequested()
                ? await RequestCredentialWithDPoP(credentialMetadata, issuerMetadata, oAuthToken)
                : await RequestCredentialWithoutDPoP(credentialMetadata, issuerMetadata, oAuthToken);
        }
        
        private async Task<AuthorizationServerMetadata> FetchAuthorizationServerMetadataAsync(OidIssuerMetadata issuerMetadata)
        {
            var credentialIssuerUrl = new Uri(issuerMetadata.CredentialIssuer);

            var getAuthServerUrl =
                !string.IsNullOrEmpty(issuerMetadata.AuthorizationServer)
                    ? issuerMetadata.AuthorizationServer
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

            return authServer;
        }
        
        private async Task<OAuthToken> RequestTokenWithDPop(
            AuthorizationServerMetadata authServerMetadata,
            string preAuthorizedCode,
            string? transactionCode = null)
        {
            var dPopKey = await _keyStore.GenerateKey();
            var dPopProofJwt = await _keyStore.GenerateDPopProofOfPossessionAsync(
                dPopKey, 
                authServerMetadata.TokenEndpoint, 
                null, 
                null
                );
            
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.AddDPopHeader(dPopProofJwt);
            
            var response = await httpClient.PostAsync(
                authServerMetadata.TokenEndpoint,
                new TokenRequest
                {
                    GrantType = "urn:ietf:params:oauth:grant-type:pre-authorized_code",
                    PreAuthorizedCode = preAuthorizedCode,
                    TransactionCode = transactionCode
                }.ToFormUrlEncoded()
                );

            var dPopNonce = await GetDPopNonce(response);

            if (!string.IsNullOrEmpty(dPopNonce))
            {
                dPopProofJwt = await _keyStore.GenerateDPopProofOfPossessionAsync(
                    dPopKey, 
                    authServerMetadata.TokenEndpoint, 
                    dPopNonce, 
                    null);
            
                httpClient.AddDPopHeader(dPopProofJwt);
                response = await httpClient.PostAsync(
                    authServerMetadata.TokenEndpoint,
                    new TokenRequest
                    {
                        GrantType = "urn:ietf:params:oauth:grant-type:pre-authorized_code",
                        PreAuthorizedCode = preAuthorizedCode,
                        TransactionCode = transactionCode
                    }.ToFormUrlEncoded());
            }
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Failed to get token. Status Code is {response.StatusCode}"
                );
            }

            var tokenResponse = DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync()) 
                                ?? throw new InvalidOperationException("Failed to deserialize the token response");

            var dPop = new DPop(dPopKey, dPopNonce);
            var oAuthToken = new OAuthToken(tokenResponse, dPop);
            
            return oAuthToken;
        }
        
        private async Task<(OidCredentialResponse credentialResponse, string keyId)> RequestCredentialWithDPoP(
            OidCredentialMetadata credentialMetadata,
            OidIssuerMetadata issuerMetadata,
            OAuthToken oAuthToken)
        {
            if (oAuthToken.DPop == null)
            {
                throw new InvalidOperationException("The DPoP Flow requires the DPoP specific parameters.");
            }
            
            var dPopProofJwt = await _keyStore.GenerateDPopProofOfPossessionAsync(
                oAuthToken.DPop.KeyId, 
                issuerMetadata.CredentialEndpoint, 
                oAuthToken.DPop.Nonce,
                oAuthToken.TokenResponse.AccessToken);
            
            var sdJwtKeyId = await _keyStore.GenerateKey();
            var keyBindingJwt = await _keyStore.GenerateKbProofOfPossessionAsync(
                sdJwtKeyId,
                issuerMetadata.CredentialIssuer,
                oAuthToken.TokenResponse.CNonce,
                "openid4vci-proof+jwt"
            );
            
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.AddAuthorizationHeader(oAuthToken);
            httpClient.AddDPopHeader(dPopProofJwt);
            
            var response = await httpClient.PostAsync(
                issuerMetadata.CredentialEndpoint,
                new StringContent(
                    content: new OidCredentialRequest
                    {
                        Format = credentialMetadata.Format,
                        CredentialDefinition = credentialMetadata.CredentialDefinition,
                        Proof = new OidProofOfPossession
                        {
                            ProofType = "jwt",
                            Jwt = keyBindingJwt
                        }
                    }.ToJson(),
                    encoding: Encoding.UTF8,
                    mediaType: "application/json"
                )
            );

            var refreshedDPopNonce = await GetDPopNonce(response);
            
            if (!string.IsNullOrEmpty(refreshedDPopNonce))
            {
                dPopProofJwt = await _keyStore.GenerateDPopProofOfPossessionAsync(
                    oAuthToken.DPop.KeyId, 
                    issuerMetadata.CredentialEndpoint, 
                    refreshedDPopNonce, 
                    oAuthToken.TokenResponse.AccessToken);
                httpClient.AddDPopHeader(dPopProofJwt);

                response = await httpClient.PostAsync(
                    issuerMetadata.CredentialEndpoint,
                    new StringContent(
                        content: new OidCredentialRequest
                        {
                            Format = credentialMetadata.Format,
                            CredentialDefinition = credentialMetadata.CredentialDefinition,
                            Proof = new OidProofOfPossession
                            {
                                ProofType = "jwt",
                                Jwt = keyBindingJwt
                            }
                        }.ToJson(),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    )
                );
            }
            
            if (!string.IsNullOrEmpty(oAuthToken.DPop.KeyId))
            {
                await _keyStore.DeleteKey(oAuthToken.DPop.KeyId);
            }
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Failed to request Credential. Status Code is {response.StatusCode}"
                );
            }

            var credentialResponse = DeserializeObject<OidCredentialResponse>(
                await response.Content.ReadAsStringAsync()
            );

            if (credentialResponse?.Credential == null)
            {
                throw new InvalidOperationException("Credential response is null.");
            }

            return (credentialResponse, sdJwtKeyId);
        }
        
        private async Task<OAuthToken> RequestTokenWithoutDPop(
            AuthorizationServerMetadata authServerMetadata,
            string preAuthorizedCode,
            string? transactionCode = null)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.PostAsync(
                authServerMetadata.TokenEndpoint,
                new TokenRequest
                {
                    GrantType = "urn:ietf:params:oauth:grant-type:pre-authorized_code",
                    PreAuthorizedCode = preAuthorizedCode,
                    TransactionCode = transactionCode
                }.ToFormUrlEncoded());

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Failed to get token. Status Code is {response.StatusCode}"
                );
            }

            var tokenResponse = DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync()) 
                                ?? throw new InvalidOperationException("Failed to deserialize the token response");

            var oAuthToken = new OAuthToken(tokenResponse);
            return oAuthToken;
        }
        
        private async Task<(OidCredentialResponse credentialResponse, string keyId)> RequestCredentialWithoutDPoP(
            OidCredentialMetadata credentialMetadata,
            OidIssuerMetadata issuerMetadata,
            OAuthToken oAuthToken)
        {
            var sdJwtKeyId = await _keyStore.GenerateKey();
            var proofOfPossession = await _keyStore.GenerateKbProofOfPossessionAsync(
                sdJwtKeyId,
                issuerMetadata.CredentialIssuer,
                oAuthToken.TokenResponse.CNonce,
                "openid4vci-proof+jwt"
            );
            
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.AddAuthorizationHeader(oAuthToken);
            
            var response = await httpClient.PostAsync(
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
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Failed to request Credential. Status Code is {response.StatusCode}"
                );
            }

            var credentialResponse = DeserializeObject<OidCredentialResponse>(
                await response.Content.ReadAsStringAsync()
            );

            if (credentialResponse?.Credential == null)
            {
                throw new InvalidOperationException("Credential response is null.");
            }

            return (credentialResponse, sdJwtKeyId);
        }

        private async Task<string?> GetDPopNonce(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var errorReason = string.IsNullOrEmpty(content) 
                ? null 
                : JObject.Parse(content)["error"]?.ToString();
            
            if (response.StatusCode 
                    is System.Net.HttpStatusCode.BadRequest 
                    or System.Net.HttpStatusCode.Unauthorized
                && errorReason == "use_dpop_nonce"
                && response.Headers.TryGetValues("DPoP-Nonce", out var dPopNonce))
            {
                return dPopNonce?.FirstOrDefault();
            }

            return null;
        }
        
        
    }
}
