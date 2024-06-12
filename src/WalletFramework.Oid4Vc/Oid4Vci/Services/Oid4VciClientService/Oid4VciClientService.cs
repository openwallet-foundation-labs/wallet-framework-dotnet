using System.Security.Cryptography;
using System.Text;
using Hyperledger.Aries.Agents;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vci.Exceptions;
using WalletFramework.Oid4Vc.Oid4Vci.Extensions;
using WalletFramework.Oid4Vc.Oid4Vci.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization;
using WalletFramework.Oid4Vc.Oid4Vci.Models.CredentialOffer.GrantTypes;
using WalletFramework.Oid4Vc.Oid4Vci.Models.CredentialRequest;
using WalletFramework.Oid4Vc.Oid4Vci.Models.CredentialResponse;
using WalletFramework.Oid4Vc.Oid4Vci.Models.DPop;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata;
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
        /// <param name="sessionRecordService">The authorization record service</param>
        /// <param name="keyStore">The key store.</param>
        public Oid4VciClientService(
            IHttpClientFactory httpClientFactory,
            ISessionRecordService sessionRecordService,
            IKeyStore keyStore)
        {
            _httpClientFactory = httpClientFactory;
            RecordService = sessionRecordService;
            _keyStore = keyStore;
        }

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IKeyStore _keyStore;
        
        private const string ErrorCodeKey = "error";
        private const string InvalidGrantError = "invalid_grant";
        private const string UseDPopNonceError = "use_dpop_nonce";
        private const string AuthorizationCodeGrantTypeIdentifier = "authorization_code";
        private const string PreAuthorizedCodeGrantTypeIdentifier = "urn:ietf:params:oauth:grant-type:pre-authorized_code";

        /// <summary>
        ///     The service responsible for wallet record operations.
        /// </summary>
        protected readonly ISessionRecordService RecordService;

        /// <inheritdoc />
        public async Task<MetadataSet> FetchMetadataAsync(Uri issuerEndpoint, string preferredLanguage)
        {
            var issuerMetadata = await FetchIssuerMetadataAsync(issuerEndpoint, "en");
            var authorizationServerMetadata = await FetchAuthorizationServerMetadataAsync(issuerMetadata);
            
            return new MetadataSet(issuerMetadata, authorizationServerMetadata);
        }
        
        /// <inheritdoc />
        public async Task<Uri> InitiateAuthentication(
            IAgentContext agentContext,
            ClientOptions clientOptions,
            MetadataSet metadataSet,
            string[] credentialConfigurationIds,
            AuthorizationCode? authorizationCode)
        {
            var authorizationCodeParameters = CreateAndStoreCodeChallenge();
            var sessionId = Guid.NewGuid().ToString();
            
            var credentialMetadatas = metadataSet.IssuerMetadata.CredentialConfigurationsSupported
                .Where(credentialConfiguration => credentialConfigurationIds.Contains(credentialConfiguration.Key))
                .Select(y => y.Value).ToList();

            var par = new PushedAuthorizationRequest(
                clientOptions.ClientId,
                clientOptions.RedirectUri + "?session=" + sessionId,
                authorizationCodeParameters.Challenge,
                authorizationCodeParameters.CodeChallengeMethod,
                credentialMetadatas?
                    .Select(credentialMetadata => new AuthorizationDetails
                    {
                        CredentialConfigurationId = credentialMetadata.Id, 
                        Format = credentialMetadata.Format, 
                        Vct = credentialMetadata.Vct, 
                        Locations = metadataSet.IssuerMetadata.AuthorizationServers
                    }).ToArray(),
                string.Join(" ", credentialMetadatas.Select(metadata => metadata.Scope)),
                authorizationCode?.IssuerState,
                clientOptions.WalletIssuer,
                null,
                null
                );
            
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            
            var response = await client.PostAsync(
                metadataSet.AuthorizationServerMetadata.PushedAuthorizationRequestEndpoint,
                par.ToFormUrlEncoded()
            );

            var parResponse = DeserializeObject<PushedAuthorizationRequestResponse>(await response.Content.ReadAsStringAsync()) 
                              ?? throw new InvalidOperationException("Failed to deserialize the PAR response.");
            
            var authorizationRequestUri = new Uri(metadataSet.AuthorizationServerMetadata.AuthorizationEndpoint 
                                                  + "?client_id=" + par.ClientId 
                                                  + "&request_uri=" + System.Net.WebUtility.UrlEncode(parResponse.RequestUri.ToString()));

            await RecordService.StoreAsync(
                agentContext, 
                sessionId, 
                authorizationCodeParameters,
                clientOptions,
                metadataSet,
                credentialConfigurationIds,
                authorizationCode);
            
            return authorizationRequestUri;
        }
        
        /// <inheritdoc />
        public async Task<(OidCredentialResponse credentialResponse, string keyId)[]> RequestCredentialAsync(
            IAgentContext context,
            IssuanceSessionParameters issuanceSessionParameters)
        {
            var record = await RecordService.GetAsync(context, issuanceSessionParameters.SessionId);

            var tokenRequest = new TokenRequest
            {
                GrantType = AuthorizationCodeGrantTypeIdentifier,
                RedirectUri = record.ClientOptions.RedirectUri + "?session=" + record.SessionId,
                CodeVerifier = record.AuthorizationCodeParameters.Verifier,
                Code = issuanceSessionParameters.Code,
                ClientId = record.ClientOptions.ClientId
            };

            var oAuthToken = record.MetadataSet.AuthorizationServerMetadata.IsDPoPSupported
                ? await RequestTokenWithDPop(
                    new Uri(record.MetadataSet.AuthorizationServerMetadata.TokenEndpoint), 
                    tokenRequest)
                : await RequestTokenWithoutDPop(
                    new Uri(record.MetadataSet.AuthorizationServerMetadata.TokenEndpoint), 
                    tokenRequest);
                
            var credentialMetadatas = record.MetadataSet.IssuerMetadata.CredentialConfigurationsSupported
                .Where(credentialConfiguration => record.CredentialConfigurationIds.Contains(credentialConfiguration.Key))
                .Select(y => y.Value);
            
            var credential = oAuthToken.IsDPoPRequested()
                ? await RequestCredentialWithDPoP(
                    credentialMetadatas.First(), 
                    record.MetadataSet.IssuerMetadata, 
                    oAuthToken,
                    record.ClientOptions)
                : await RequestCredentialWithoutDPoP(
                    credentialMetadatas.First(), 
                    record.MetadataSet.IssuerMetadata, 
                    oAuthToken,
                    record.ClientOptions);
            
            await RecordService.DeleteAsync(context, record.Id);
            
            //TODO: Return multiple credentials
            return new[] { credential };
        }
        
        /// <inheritdoc />
        public async Task<(OidCredentialResponse credentialResponse, string keyId)[]> RequestCredentialAsync(
            MetadataSet metadataSet,
            OidCredentialMetadata credentialMetadata,
            string preAuthorizedCode,
            string? transactionCode)
        {
            var tokenRequest = new TokenRequest
            {
                GrantType = PreAuthorizedCodeGrantTypeIdentifier,
                PreAuthorizedCode = preAuthorizedCode,
                TransactionCode = transactionCode
            };
                
            var oAuthToken = metadataSet.AuthorizationServerMetadata.IsDPoPSupported
                ? await RequestTokenWithDPop(
                    new Uri(metadataSet.AuthorizationServerMetadata.TokenEndpoint), 
                    tokenRequest)
                : await RequestTokenWithoutDPop(
                    new Uri(metadataSet.AuthorizationServerMetadata.TokenEndpoint),
                    tokenRequest);
            
            var credential = oAuthToken.IsDPoPRequested()
                ? await RequestCredentialWithDPoP(
                    credentialMetadata, 
                    metadataSet.IssuerMetadata, 
                    oAuthToken)
                : await RequestCredentialWithoutDPoP(
                    credentialMetadata, 
                    metadataSet.IssuerMetadata, 
                    oAuthToken);
            
            //TODO: Return multiple credentials
            return new[] { credential };
        }

        /// <inheritdoc />
        private async Task<OidIssuerMetadata> FetchIssuerMetadataAsync(Uri endpoint, string preferredLanguage)
        {
            var baseEndpoint = endpoint
                .AbsolutePath
                .EndsWith("/")
                ? endpoint
                : new Uri(endpoint.OriginalString + "/");

            var metadataUrl = new Uri(baseEndpoint, ".well-known/openid-credential-issuer");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Accept-Language", preferredLanguage);
            
            var response = await client.GetAsync(metadataUrl);

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
        
        private async Task<AuthorizationServerMetadata> FetchAuthorizationServerMetadataAsync(OidIssuerMetadata issuerMetadata)
        {
            var credentialIssuerUrl = new Uri(issuerMetadata.CredentialIssuer);

            var getAuthServerUrl =
                !string.IsNullOrEmpty(issuerMetadata.AuthorizationServers?.First())
                    ? issuerMetadata.AuthorizationServers?.First()
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
            Uri authServerTokenEndpoint,
            TokenRequest tokenRequest)
        {
            var dPopKey = await _keyStore.GenerateKey();
            var dPopProofJwt = await _keyStore.GenerateDPopProofOfPossessionAsync(
                dPopKey,
                authServerTokenEndpoint.ToString(),
                null,
                null
                );
            
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.AddDPopHeader(dPopProofJwt);
            
            var response = await httpClient.PostAsync(
                authServerTokenEndpoint,
                tokenRequest.ToFormUrlEncoded()
                );

            await ThrowIfInvalidGrantError(response);
            
            var dPopNonce = await GetDPopNonce(response);

            if (!string.IsNullOrEmpty(dPopNonce))
            {
                dPopProofJwt = await _keyStore.GenerateDPopProofOfPossessionAsync(
                    dPopKey, 
                    authServerTokenEndpoint.ToString(), 
                    dPopNonce, 
                    null);
            
                httpClient.AddDPopHeader(dPopProofJwt);
                response = await httpClient.PostAsync(
                    authServerTokenEndpoint,
                    tokenRequest.ToFormUrlEncoded());
            }
            
            await ThrowIfInvalidGrantError(response);
            
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
        
        private async Task<OAuthToken> RequestTokenWithoutDPop(
            Uri authServerTokenEndpoint,
            TokenRequest tokenRequest)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.PostAsync(
                authServerTokenEndpoint,
                tokenRequest.ToFormUrlEncoded());

            await ThrowIfInvalidGrantError(response);
            
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
        
        private async Task<(OidCredentialResponse credentialResponse, string keyId)> RequestCredentialWithDPoP(
            OidCredentialMetadata credentialMetadata,
            OidIssuerMetadata issuerMetadata,
            OAuthToken oAuthToken,
            ClientOptions? clientOptions = null)
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
                "openid4vci-proof+jwt",
                null,
                clientOptions?.ClientId
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
                        Vct = credentialMetadata.Vct,
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
                            Vct = credentialMetadata.Vct,
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
        
        private async Task<(OidCredentialResponse credentialResponse, string keyId)> RequestCredentialWithoutDPoP(
            OidCredentialMetadata credentialMetadata,
            OidIssuerMetadata issuerMetadata,
            OAuthToken oAuthToken,
            ClientOptions? clientOptions = null)
        {
            var sdJwtKeyId = await _keyStore.GenerateKey();
            var proofOfPossession = await _keyStore.GenerateKbProofOfPossessionAsync(
                sdJwtKeyId,
                issuerMetadata.CredentialIssuer,
                oAuthToken.TokenResponse.CNonce,
                "openid4vci-proof+jwt",
                null,
                clientOptions?.ClientId
            );
            
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.AddAuthorizationHeader(oAuthToken);
            
            var response = await httpClient.PostAsync(
                issuerMetadata.CredentialEndpoint,
                new StringContent(
                    content: new OidCredentialRequest
                    {
                        Format = credentialMetadata.Format,
                        Vct = credentialMetadata.Vct,
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
        
        private async Task ThrowIfInvalidGrantError(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var errorReason = string.IsNullOrEmpty(content) 
                ? null
                : JObject.Parse(content)[ErrorCodeKey]?.ToString();

            if (response.StatusCode is System.Net.HttpStatusCode.BadRequest
                && errorReason == InvalidGrantError)
            {
                throw new Oid4VciInvalidGrantException(response.StatusCode);
            }
        }

        private async Task<string?> GetDPopNonce(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var errorReason = string.IsNullOrEmpty(content) 
                ? null 
                : JObject.Parse(content)[ErrorCodeKey]?.ToString();
            
            if (response.StatusCode 
                    is System.Net.HttpStatusCode.BadRequest 
                    or System.Net.HttpStatusCode.Unauthorized
                && errorReason == UseDPopNonceError
                && response.Headers.TryGetValues("DPoP-Nonce", out var dPopNonce))
            {
                return dPopNonce?.FirstOrDefault();
            }

            return null;
        }
        
        private AuthorizationCodeParameters CreateAndStoreCodeChallenge()
        {
            var rng = new RNGCryptoServiceProvider();
            byte[] randomNumber = new byte[32];
            rng.GetBytes(randomNumber);
            
            var codeVerifier = Base64UrlEncoder.Encode(randomNumber);
            
            var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            
            var codeChallenge = Base64UrlEncoder.Encode(bytes);

            return new AuthorizationCodeParameters(codeChallenge, codeVerifier);
        }
    }
}
