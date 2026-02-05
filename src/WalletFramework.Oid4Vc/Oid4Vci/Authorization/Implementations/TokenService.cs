using LanguageExt;
using OneOf;
using WalletFramework.Core.Cryptography.Abstractions;
using WalletFramework.Oid4Vc.ClientAttestations;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Models;
using static Newtonsoft.Json.JsonConvert;

namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.Implementations;

internal class TokenService(
    ICredentialNonceService credentialNonceService,
    IDPopHttpClient dPopHttpClient,
    IHttpClientFactory httpClientFactory,
    IKeyStore keyStore) : ITokenService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task<OneOf<OAuthToken, DPopToken>> RequestToken(
        AuthorizationServerMetadata metadata,
        Option<ClientAttestation> clientAttestation,
        Option<CredentialNonceEndpoint> credentialNonceEndpoint,
        TokenRequest tokenRequest)
    {
        if (metadata.IsDPoPSupported)
        {
            var keyId = await clientAttestation.Match(
                Some: attestation => Task.FromResult(attestation.WalletAttestation.KeyId),
                None: async () => await keyStore.GenerateKey());

            var config = new DPopConfig(keyId, metadata.TokenEndpoint);

            var uri = new Uri(metadata.TokenEndpoint);
            
            var result = await dPopHttpClient.Post(
                config,
                tokenRequest.ToFormUrlEncoded,
                clientAttestation,
                uri);
            
            var token = DeserializeObject<OAuthToken>(await result.ResponseMessage.Content.ReadAsStringAsync()) 
                        ?? throw new InvalidOperationException("Failed to deserialize the token response");

            await credentialNonceEndpoint.IfSomeAsync(async endpoint =>
            {
                var credentialNonce = await credentialNonceService.GetCredentialNonce(endpoint);
                token = token with { CNonce = credentialNonce.Value };
            });
            
            return new DPopToken(
                token, 
                new DPop.Models.DPop(result.Config));
        }
        else
        {
            clientAttestation.IfSome(attestation => _httpClient.AddClientAttestation(attestation));

            var response = await _httpClient.PostAsync(
                metadata.TokenEndpoint,
                tokenRequest.ToFormUrlEncoded());
            
            var token = DeserializeObject<OAuthToken>(await response.Content.ReadAsStringAsync()) 
                        ?? throw new InvalidOperationException("Failed to deserialize the token response");
            
            await credentialNonceEndpoint.IfSomeAsync(async endpoint =>
            {
                var credentialNonce = await credentialNonceService.GetCredentialNonce(endpoint);
                token = token with { CNonce = credentialNonce.Value };
            });
            
            return token;
        }
    }
}
