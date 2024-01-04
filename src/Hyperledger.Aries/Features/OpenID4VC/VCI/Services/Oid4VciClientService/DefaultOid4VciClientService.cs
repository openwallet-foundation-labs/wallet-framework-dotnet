#nullable enable

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Hyperledger.Aries.Extensions;
using Hyperledger.Aries.Features.OpenId4Vc.KeyStore.Services;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Authorization;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialRequest;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialResponse;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Credential;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Issuer;
using Newtonsoft.Json;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vci.Services.Oid4VciClientService
{
    /// <inheritdoc />
    public class DefaultOid4VciClientService : IOid4VciClientService
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultOid4VciClientService" /> class.
        /// </summary>
        /// <param name="httpClientFactory">
        ///     The factory to create instances of <see cref="HttpClient" />. Used for making HTTP
        ///     requests.
        /// </param>
        /// <param name="keyStore">The key store.</param>
        public DefaultOid4VciClientService(
            IHttpClientFactory httpClientFactory,
            IKeyStore keyStore)
        {
            _httpClientFactory = httpClientFactory;
            _keyStore = keyStore;
        }

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IKeyStore _keyStore;

        /// <inheritdoc />
        public virtual async Task<OidIssuerMetadata> FetchIssuerMetadataAsync(Uri endpoint)
        {
            var httpClient = _httpClientFactory.CreateClient();
            
            var baseEndpoint = endpoint.AbsolutePath.EndsWith("/") ? endpoint : new Uri(endpoint.OriginalString + "/");
            var metadataUrl = new Uri(baseEndpoint, ".well-known/openid-credential-issuer");

            var response = await httpClient.GetAsync(metadataUrl);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<OidIssuerMetadata>(responseString)
                       ?? throw new InvalidOperationException(
                           "Failed to deserialize the issuer metadata. JSON: " +
                           responseString);
            }

            throw new HttpRequestException(
                $"Failed to get Issuer metadata. Status code is {response.StatusCode} with message {responseString}");
        }

        /// <inheritdoc />
        public virtual async Task<(OidCredentialResponse, string)> RequestCredentialAsync(
            string credentialIssuer,
            string clientNonce,
            string type,
            string credentialEndpoint,
            TokenResponse tokenResponse)
        {
            var keyId = await _keyStore.GenerateKey();
            var proofOfPossession = await _keyStore.GenerateProofOfPossessionAsync(
                keyId, credentialIssuer, clientNonce, "openid4vci-proof+jwt");

            var credentialRequest = new OidCredentialRequest
            {
                Format = "vc+sd-jwt",
                CredentialDefinition = new OidCredentialDefinition
                {
                    Vct = type
                },
                Proof = new OidProofOfPossession
                {
                    ProofType = "jwt",
                    Jwt = proofOfPossession
                }
            };

            var requestData = new StringContent(credentialRequest.ToJson(), Encoding.UTF8, "application/json");
            
            var httpClient = _httpClientFactory.CreateClient();
            
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization",
                $"{tokenResponse.TokenType} {tokenResponse.AccessToken}");
            
            var responseData = await httpClient.PostAsync(new Uri(credentialEndpoint), requestData);

            var responseString = await responseData.Content.ReadAsStringAsync();
            if (!responseData.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Failed to request Credential. Status Code is {responseData.StatusCode} with message {responseString}");
            }

            var credentialResponse = JsonConvert.DeserializeObject<OidCredentialResponse>(responseString)
                                     ?? throw new InvalidOperationException(
                                         "Failed to deserialize the credential response. JSON: " +
                                         responseString);

            if (credentialResponse.Credential == null)
            {
                throw new InvalidOperationException("Credential in response is null.");
            }

            return (credentialResponse, keyId);
        }

        /// <inheritdoc />
        public virtual async Task<TokenResponse> RequestTokenAsync(OidIssuerMetadata metadata, string preAuthorizedCode,
            string? transactionCode = null)
        {
            var authServer = await GetAuthorizationServerMetadata(metadata);

            var tokenRequest = new TokenRequest
            {
                GrantType = "urn:ietf:params:oauth:grant-type:pre-authorized_code",
                PreAuthorizedCode = preAuthorizedCode
            };

            if (!string.IsNullOrEmpty(transactionCode))
            {
                tokenRequest.TransactionCode = transactionCode;
            }

            var formUrlEncodedRequest = tokenRequest.ToFormUrlEncoded();

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsync(authServer.TokenEndpoint, formUrlEncodedRequest);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TokenResponse>(responseString) ??
                       throw new InvalidOperationException("Failed to deserialize the token response. JSON: " +
                                                           responseString);
            }

            throw new HttpRequestException(
                $"Failed to get token. Status Code is {response.StatusCode} with message {responseString}");
        }

        private async Task<AuthorizationServerMetadata> GetAuthorizationServerMetadata(OidIssuerMetadata metadata)
        {
            var endpointUrl = GetAuthorizationServerUrl(metadata);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(endpointUrl);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<AuthorizationServerMetadata>(responseString)
                       ?? throw new InvalidOperationException(
                           "Failed to deserialize the authorization server metadata. JSON: " + responseString);
            }

            throw new HttpRequestException(
                $"Failed to get authorization server metadata. Status Code is: {response.StatusCode} with message {responseString}");
        }

        private static string GetAuthorizationServerUrl(OidIssuerMetadata metadata)
        {
            if (!string.IsNullOrEmpty(metadata.AuthorizationServer))
            {
                return metadata.AuthorizationServer;
            }

            var credentialIssuerUrl = new Uri(metadata.CredentialIssuer);
            if (string.IsNullOrEmpty(credentialIssuerUrl.AbsolutePath) || credentialIssuerUrl.AbsolutePath == "/")
            {
                return
                    $"{credentialIssuerUrl.GetLeftPart(UriPartial.Authority)}/.well-known/oauth-authorization-server";
            }

            var trimmedPath = credentialIssuerUrl.AbsolutePath.TrimEnd('/');
            return
                $"{credentialIssuerUrl.GetLeftPart(UriPartial.Authority)}/.well-known/oauth-authorization-server{trimmedPath}";
        }
    }
}
