using System.Text;
using LanguageExt;
using OneOf;
using WalletFramework.Core.Cryptography.Abstractions;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Uri;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.SdJwt;
using WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Models.Mdoc;
using WalletFramework.Oid4Vc.Oid4Vci.Extensions;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Models.SdJwt;
using WalletFramework.Oid4Vc.Oid4Vci.CredResponse;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Implementations;

public class CredentialRequestService : ICredentialRequestService
{
    public CredentialRequestService(
        HttpClient httpClient,
        IDPopHttpClient dPopHttpClient,
        ISdJwtSignerService sdJwtSignerService,
        IKeyStore keyStore)
    {
        _dPopHttpClient = dPopHttpClient;
        _sdJwtSignerService = sdJwtSignerService;
        _keyStore = keyStore;
        _httpClient = httpClient;
    }

    private readonly HttpClient _httpClient;
    private readonly IDPopHttpClient _dPopHttpClient;
    private readonly ISdJwtSignerService _sdJwtSignerService;
    private readonly IKeyStore _keyStore;

    private async Task<CredentialRequest> CreateCredentialRequest(
        KeyId keyId,
        Format format,
        IssuerMetadata issuerMetadata,
        OneOf<OAuthToken, DPopToken> token,
        Option<ClientOptions> clientOptions)
    {
        var cNonce = token.Match(
            oauthToken => oauthToken.CNonce,
            dPopToken => dPopToken.Token.CNonce);

        var keyBindingJwt = await _sdJwtSignerService.GenerateKbProofOfPossessionAsync(
            keyId,
            issuerMetadata.CredentialIssuer.ToString(),
            cNonce,
            "openid4vci-proof+jwt",
            null,
            clientOptions.ToNullable()?.ClientId);

        var proof = new ProofOfPossession
        {
            ProofType = "jwt",
            Jwt = keyBindingJwt
        };

        return new CredentialRequest(proof, format);
    }

    async Task<Validation<CredentialResponse>> ICredentialRequestService.RequestCredentials(
        OneOf<SdJwtConfiguration, MdocConfiguration> configuration,
        IssuerMetadata issuerMetadata,
        OneOf<OAuthToken, DPopToken> token,
        Option<ClientOptions> clientOptions)
    {
        var keyId = await _keyStore.GenerateKey();

        var requestJson = await configuration.Match(
            async sdJwt =>
            {
                var vciRequest = await CreateCredentialRequest(
                    keyId,
                    sdJwt.Format,
                    issuerMetadata,
                    token,
                    clientOptions);
                
                var result = new SdJwtCredentialRequest(vciRequest, sdJwt.Vct);
                return result.EncodeToJson();
            },
            async mdoc =>
            {
                var vciRequest = await CreateCredentialRequest(
                    keyId,
                    mdoc.Format,
                    issuerMetadata,
                    token,
                    clientOptions);
                
                var result = new MdocCredentialRequest(vciRequest, mdoc);
                return result.AsJson();
            }
        );

        var content = new StringContent(
            requestJson,
            Encoding.UTF8,
            "application/json");

        var response = await token.Match(
            async authToken => await _httpClient
                .WithAuthorizationHeader(authToken)
                .PostAsync(issuerMetadata.CredentialEndpoint, content),
            async dPopToken =>
            {
                var config = dPopToken.DPop.Config with
                {
                    Audience = issuerMetadata.CredentialEndpoint.ToStringWithoutTrail(),
                    OAuthToken = dPopToken.Token
                };

                var dPopResponse = await _dPopHttpClient.Post(
                    issuerMetadata.CredentialEndpoint,
                    content,
                    config);

                return dPopResponse.ResponseMessage;
            });

        var responseContent = await response.Content.ReadAsStringAsync();

        return 
            from jObject in JsonFun.ParseAsJObject(responseContent)
            from credResponse in CredentialResponse.ValidCredentialResponse(jObject, keyId)
            select credResponse;
    }
}
