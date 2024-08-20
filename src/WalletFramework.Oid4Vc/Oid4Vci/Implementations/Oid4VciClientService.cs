using System.Security.Cryptography;
using System.Text;
using Hyperledger.Aries.Agents;
using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;
using OneOf;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Localization;
using WalletFramework.MdocVc;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Models;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;
using static Newtonsoft.Json.JsonConvert;

namespace WalletFramework.Oid4Vc.Oid4Vci.Implementations;

/// <inheritdoc />
public class Oid4VciClientService : IOid4VciClientService
{
    private const string AuthorizationCodeGrantTypeIdentifier = "authorization_code";
    private const string PreAuthorizedCodeGrantTypeIdentifier = "urn:ietf:params:oauth:grant-type:pre-authorized_code";

    /// <summary>
    ///     Initializes a new instance of the <see cref="Oid4VciClientService" /> class.
    /// </summary>
    /// <param name="credentialOfferService"></param>
    /// <param name="credentialRequestService">The credential request service. </param>
    /// <param name="issuerMetadataService"></param>
    /// <param name="httpClientFactory">
    ///     The factory to create instances of <see cref="_httpClient" />. Used for making HTTP
    ///     requests.
    /// </param>
    /// <param name="authFlowSessionAuthFlowSessionStorage">The authorization record service</param>
    /// <param name="sdJwtService"></param>
    /// <param name="tokenService">The token service.</param>
    /// <param name="agentProvider"></param>
    /// <param name="mdocStorage"></param>
    public Oid4VciClientService(
        IAgentProvider agentProvider,
        ICredentialOfferService credentialOfferService,
        ICredentialRequestService credentialRequestService,
        IMdocStorage mdocStorage,
        IIssuerMetadataService issuerMetadataService,
        IHttpClientFactory httpClientFactory,
        IAuthFlowSessionStorage authFlowSessionAuthFlowSessionStorage,
        ISdJwtVcHolderService sdJwtService,
        ITokenService tokenService)
    {
        _agentProvider = agentProvider;
        _credentialOfferService = credentialOfferService;
        _credentialRequestService = credentialRequestService;
        _httpClient = httpClientFactory.CreateClient();
        _issuerMetadataService = issuerMetadataService;
        _mdocStorage = mdocStorage;
        _authFlowSessionStorage = authFlowSessionAuthFlowSessionStorage;
        _sdJwtService = sdJwtService;
        _tokenService = tokenService;
    }

    private readonly HttpClient _httpClient;
    private readonly IAgentProvider _agentProvider;
    private readonly IAuthFlowSessionStorage _authFlowSessionStorage;
    private readonly ICredentialOfferService _credentialOfferService;
    private readonly ICredentialRequestService _credentialRequestService;
    private readonly IIssuerMetadataService _issuerMetadataService;
    private readonly IMdocStorage _mdocStorage;
    private readonly ISdJwtVcHolderService _sdJwtService;
    private readonly ITokenService _tokenService;
    
    /// <inheritdoc />
    public async Task<Uri> InitiateAuthFlow(CredentialOfferMetadata offer, ClientOptions clientOptions)
    {
        var authorizationCodeParameters = CreateAndStoreCodeChallenge();
        var sessionId = AuthFlowSessionState.CreateAuthFlowSessionState();
        var issuerMetadata = offer.IssuerMetadata;
            
        var scopes = offer
            .CredentialOffer
            .CredentialConfigurationIds
            .Select(id => issuerMetadata.CredentialConfigurationsSupported[id])
            .Select(oneOf => oneOf.Match(
                sdJwt => sdJwt.CredentialConfiguration.Scope.OnSome(scope => scope.ToString()),
                mdoc => mdoc.CredentialConfiguration.Scope.OnSome(scope => scope.ToString())
            ))
            .Where(option => option.IsSome)
            .Select(option => option.Fallback(string.Empty));
        
        var scope = string.Join(" ", scopes);
        
        var authorizationDetails = issuerMetadata
            .CredentialConfigurationsSupported
            .Where(config => offer.CredentialOffer.CredentialConfigurationIds.Contains(config.Key))
            .Select(pair => pair.Value.Match(
                sdJwt => new AuthorizationDetails(
                    null,
                    sdJwt.Vct.ToString(),
                    pair.Key.ToString(),
                    issuerMetadata.AuthorizationServers.ToNullable()?.Select(id => id.ToString()).ToArray(),
                    null
                ),
                mdoc => new AuthorizationDetails(
                    null,
                    null,
                    pair.Key.ToString(),
                    issuerMetadata.AuthorizationServers.ToNullable()?.Select(id => id.ToString()).ToArray(),
                    mdoc.DocType.ToString()))
            );

        var authCode =
            from grants in offer.CredentialOffer.Grants
            from code in grants.AuthorizationCode
            select code;

        var issuerState =
            from code in authCode
            from issState in code.IssuerState
            select issState;

        var par = new PushedAuthorizationRequest(
            sessionId,
            clientOptions,
            authorizationCodeParameters,
            authorizationDetails.ToArray(),
            scope,
            issuerState.ToNullable(),
            null,
            null);

        var authServerMetadata = 
            await FetchAuthorizationServerMetadataAsync(issuerMetadata);
            
        _httpClient.DefaultRequestHeaders.Clear();
        var response = await _httpClient.PostAsync(
            authServerMetadata.PushedAuthorizationRequestEndpoint,
            par.ToFormUrlEncoded()
        );

        var parResponse = DeserializeObject<PushedAuthorizationRequestResponse>(await response.Content.ReadAsStringAsync()) 
                          ?? throw new InvalidOperationException("Failed to deserialize the PAR response.");
            
        var authorizationRequestUri = new Uri(authServerMetadata.AuthorizationEndpoint 
                                              + "?client_id=" + par.ClientId 
                                              + "&request_uri=" + System.Net.WebUtility.UrlEncode(parResponse.RequestUri.ToString()));

        var authorizationData = new AuthorizationData(
            clientOptions,
            issuerMetadata,
            authServerMetadata,
            offer.CredentialOffer.CredentialConfigurationIds);

        var context = await _agentProvider.GetContextAsync();
        await _authFlowSessionStorage.StoreAsync(
            context,
            authorizationData,
            authorizationCodeParameters,
            sessionId);
            
        return authorizationRequestUri;
    }

    public async Task<Uri> InitiateAuthFlow(Uri uri, ClientOptions clientOptions, Option<Locale> language)
    {
        var locale = language.Match(
            some => some,
            () => Constants.DefaultLocale);
        
        var issuerMetadata = _issuerMetadataService.ProcessMetadata(uri, locale);
        
        return await issuerMetadata.Match(
            async validIssuerMetadata =>
            {
                var sessionId = AuthFlowSessionState.CreateAuthFlowSessionState();
                var authorizationCodeParameters = CreateAndStoreCodeChallenge();

                var scope = validIssuerMetadata.CredentialConfigurationsSupported.First().Value.Match(
                    sdJwtConfig => sdJwtConfig.CredentialConfiguration.Scope.OnSome(scope => scope.ToString()),
                    mdDocConfig => mdDocConfig.CredentialConfiguration.Scope.OnSome(scope => scope.ToString())
                );
                
                var par = new PushedAuthorizationRequest(
                    sessionId,
                    clientOptions,
                    authorizationCodeParameters,
                    null,
                    scope.ToNullable(),
                    null,
                    null,
                    null);
                
                var authServerMetadata = 
                    await FetchAuthorizationServerMetadataAsync(validIssuerMetadata);
            
                _httpClient.DefaultRequestHeaders.Clear();
                var response = await _httpClient.PostAsync(
                    authServerMetadata.PushedAuthorizationRequestEndpoint,
                    par.ToFormUrlEncoded()
                );

                var parResponse = DeserializeObject<PushedAuthorizationRequestResponse>(await response.Content.ReadAsStringAsync()) 
                                  ?? throw new InvalidOperationException("Failed to deserialize the PAR response.");
            
                var authorizationRequestUri = new Uri(authServerMetadata.AuthorizationEndpoint 
                                                      + "?client_id=" + par.ClientId 
                                                      + "&request_uri=" + System.Net.WebUtility.UrlEncode(parResponse.RequestUri.ToString()));

                //TODO: Select multiple configurationIds
                var authorizationData = new AuthorizationData(
                    clientOptions,
                    validIssuerMetadata,
                    authServerMetadata,
                    validIssuerMetadata.CredentialConfigurationsSupported.Keys.ToList());

                var context = await _agentProvider.GetContextAsync();
                await _authFlowSessionStorage.StoreAsync(
                    context,
                    authorizationData,
                    authorizationCodeParameters,
                    sessionId);
            
                return authorizationRequestUri;
            },
            _ => throw new Exception("Fetching Issuer metadata failed")
            );
    }

    public async Task<Validation<OneOf<SdJwtRecord, MdocRecord>>> AcceptOffer(CredentialOfferMetadata credentialOfferMetadata, string? transactionCode)
    {
        var issuerMetadata = credentialOfferMetadata.IssuerMetadata;
        // TODO: Support multiple configs
        var configId = credentialOfferMetadata.CredentialOffer.CredentialConfigurationIds.First();
        var configuration = issuerMetadata.CredentialConfigurationsSupported[configId];
        var preAuthorizedCode =
            from grants in credentialOfferMetadata.CredentialOffer.Grants
            from preAuthCode in grants.PreAuthorizedCode
            select preAuthCode.Value;
        
        var tokenRequest = new TokenRequest
        {
            GrantType = PreAuthorizedCodeGrantTypeIdentifier,
            PreAuthorizedCode = preAuthorizedCode.ToNullable(),
            TransactionCode = transactionCode
        };

        var authorizationServerMetadata = await FetchAuthorizationServerMetadataAsync(issuerMetadata);

        var token = await _tokenService.RequestToken(
            tokenRequest,
            authorizationServerMetadata);

        var validResponse = await _credentialRequestService.RequestCredentials(
            configuration,
            issuerMetadata,
            token,
            Option<ClientOptions>.None);
        
        var result =
            from response in validResponse
            let credentialOrTransactionId = response.CredentialOrTransactionId
            select credentialOrTransactionId.Match(
                async credential => await credential.Value.Match<Task<OneOf<SdJwtRecord, MdocRecord>>>(
                    async sdJwt =>
                    {
                        var record = sdJwt.Decoded.ToRecord(configuration.AsT0, response.KeyId);
                        var context = await _agentProvider.GetContextAsync();
                        await _sdJwtService.AddAsync(context, record);
                        return record;
                    },
                    async mdoc =>
                    {
                        var displays = MdocFun.CreateMdocDisplays(configuration.AsT1);
                        var record = mdoc.Decoded.ToRecord(displays, response.KeyId);
                        await _mdocStorage.Add(record);
                        return record;
                    }),
                // ReSharper disable once UnusedParameter.Local
                transactionId => throw new NotImplementedException());
        
        return await result.OnSuccess(task => task);
    }

    public async Task<Validation<CredentialOfferMetadata>> ProcessOffer(Uri credentialOffer, Option<Locale> language)
    {
        var locale = language.Match(
            some => some,
            () => Constants.DefaultLocale);
        
        var result =
            from offer in _credentialOfferService.ProcessCredentialOffer(credentialOffer, locale)
            from metadata in _issuerMetadataService.ProcessMetadata(offer.CredentialIssuer, locale)
            select new CredentialOfferMetadata(offer, metadata);

        return await result;
    }

    /// <inheritdoc />
    public async Task<Validation<List<OneOf<SdJwtRecord, MdocRecord>>>> RequestCredential(IssuanceSession issuanceSession)
    {
        var context = await _agentProvider.GetContextAsync();
        
        var session = await _authFlowSessionStorage.GetAsync(context, issuanceSession.AuthFlowSessionState);
        
        var credConfiguration = session
            .AuthorizationData
            .IssuerMetadata
            .CredentialConfigurationsSupported
            .Where(config => session.AuthorizationData.CredentialConfigurationIds.Contains(config.Key))
            .Select(pair => pair.Value);
        
        var scope = session
            .AuthorizationData
            .IssuerMetadata
            .CredentialConfigurationsSupported.First().Value.Match(
                sdJwtConfig => sdJwtConfig.CredentialConfiguration.Scope.OnSome(scope => scope.ToString()), 
                mdDocConfig => mdDocConfig.CredentialConfiguration.Scope.OnSome(scope => scope.ToString()));
        
        var tokenRequest = new TokenRequest
        {
            GrantType = AuthorizationCodeGrantTypeIdentifier,
            RedirectUri = session.AuthorizationData.ClientOptions.RedirectUri,
            CodeVerifier = session.AuthorizationCodeParameters.Verifier,
            Code = issuanceSession.Code,
            Scope = scope.ToNullable(),
            ClientId = session.AuthorizationData.ClientOptions.ClientId
        };
        
        var token = await _tokenService.RequestToken(
            tokenRequest,
            session.AuthorizationData.AuthorizationServerMetadata);

        List<OneOf<SdJwtRecord, MdocRecord>> credentials = new();
        //TODO: Make sure that it does not always request all available credConfigurations
        foreach (var configuration in credConfiguration)
        {
            var validResponse = await _credentialRequestService.RequestCredentials(
                configuration,
                session.AuthorizationData.IssuerMetadata,
                token,
                session.AuthorizationData.ClientOptions);
            
            var result =
                from response in validResponse
                let cNonce = response.CNonce
                let credentialOrTransactionId = response.CredentialOrTransactionId
                select credentialOrTransactionId.Match(
                    async credential => await credential.Value.Match<Task<OneOf<SdJwtRecord, MdocRecord>>>(
                        async sdJwt =>
                        {
                            token = token.Match<OneOf<OAuthToken, DPopToken>>(
                                oAuth => oAuth with { CNonce = cNonce.ToNullable()},
                                dPop => dPop with { Token = dPop.Token with {CNonce = cNonce.ToNullable()}});
                            
                            var record = sdJwt.Decoded.ToRecord(configuration.AsT0, response.KeyId);
                            await _sdJwtService.AddAsync(context, record);
                            return record;
                        },
                        async mdoc =>
                        {
                            token = token.Match<OneOf<OAuthToken, DPopToken>>(
                                oAuth => oAuth with { CNonce = cNonce.ToNullable()},
                                dPop => dPop with { Token = dPop.Token with {CNonce = cNonce.ToNullable()}});
                            
                            var displays = MdocFun.CreateMdocDisplays(configuration.AsT1);
                            var record = mdoc.Decoded.ToRecord(displays, response.KeyId);
                            await _mdocStorage.Add(record);
                            return record;
                        }),
                    // ReSharper disable once UnusedParameter.Local
                    transactionId => throw new NotImplementedException());

            await result.OnSuccess(async task => credentials.Add(await task));
        }
        
        await _authFlowSessionStorage.DeleteAsync(context, session.AuthFlowSessionState);
        
        return credentials;
    }

    private static AuthorizationCodeParameters CreateAndStoreCodeChallenge()
    {
        var rng = new RNGCryptoServiceProvider();
        var randomNumber = new byte[32];
        rng.GetBytes(randomNumber);

        var codeVerifier = Base64UrlEncoder.Encode(randomNumber);

        var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));

        var codeChallenge = Base64UrlEncoder.Encode(bytes);

        return new AuthorizationCodeParameters(codeChallenge, codeVerifier);
    }

    private async Task<AuthorizationServerMetadata> FetchAuthorizationServerMetadataAsync(IssuerMetadata issuerMetadata)
    {
        Uri credentialIssuer = issuerMetadata.CredentialIssuer;

        var authServerUrl = issuerMetadata.AuthorizationServers.Match(
            servers => CreateAuthorizationServerMetadataUri(servers.First()),
            () => CreateAuthorizationServerMetadataUri(credentialIssuer));

        var getAuthServerResponse = await _httpClient.GetAsync(authServerUrl);

        if (!getAuthServerResponse.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Failed to get authorization server metadata. Status Code is: {getAuthServerResponse.StatusCode}"
            );

        var content = await getAuthServerResponse.Content.ReadAsStringAsync();

        var authServer = DeserializeObject<AuthorizationServerMetadata>(content)
                         ?? throw new InvalidOperationException(
                             "Failed to deserialize the authorization server metadata.");

        return authServer;
    }
    
    private static Uri CreateAuthorizationServerMetadataUri(Uri authorizationServerUri)
    {
        string result;
        if (string.IsNullOrWhiteSpace(authorizationServerUri.AbsolutePath) || authorizationServerUri.AbsolutePath == "/")
            result = $"{authorizationServerUri.GetLeftPart(UriPartial.Authority)}/.well-known/oauth-authorization-server";
        else
            result = $"{authorizationServerUri.GetLeftPart(UriPartial.Authority)}/.well-known/oauth-authorization-server" + authorizationServerUri.AbsolutePath.TrimEnd('/');
        return new Uri(result);
    }
}
