using System.Security.Cryptography;
using System.Text;
using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;
using OneOf;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Localization;
using WalletFramework.Core.String;
using WalletFramework.MdocLib;
using WalletFramework.MdocVc;
using WalletFramework.Oid4Vc.ClientAttestation;
using WalletFramework.Oid4Vc.CredentialSet.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.Storage;
using static Newtonsoft.Json.JsonConvert;
using WalletFramework.Oid4Vc.CredentialSet.Persistence;
using WalletFramework.Core.Credentials;
using WalletFramework.MdocVc.Persistence;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Persistence;
using WalletFramework.SdJwtVc;
using WalletFramework.SdJwtVc.Persistence;

namespace WalletFramework.Oid4Vc.Oid4Vci.Implementations;

/// <summary>
///     Initializes a new instance of the <see cref="Oid4VciClientService" /> class.
/// </summary>    
/// <param name="authFlowSessionRepository">The authorization flow session storage service.</param>
/// <param name="clientAttestationService">The client attestation service.</param>
/// <param name="credentialOfferService">The credential offer service.</param>
/// <param name="credentialRequestService">The credential request service.</param>
/// <param name="credentialDataSetRepository">The repository for storing credential data sets.</param>
/// <param name="sdJwtCredentialRepository">The repository for storing SD-JWT credentials.</param>
/// <param name="mdocCredentialRepository">The repository for storing mDOC credentials.</param>
/// <param name="httpClientFactory">The factory to create HTTP client instances.</param>
/// <param name="issuerMetadataService">The issuer metadata service.</param>
/// <param name="credentialNonceService">The credential nonce service.</param>
/// <param name="tokenService">The token service.</param>
public class Oid4VciClientService(
    IClientAttestationService clientAttestationService,
    ICredentialNonceService credentialNonceService,
    ICredentialOfferService credentialOfferService,
    ICredentialRequestService credentialRequestService,
    IDomainRepository<AuthFlowSession, AuthFlowSessionRecord, AuthFlowSessionState> authFlowSessionRepository,
    IDomainRepository<CredentialDataSet, CredentialDataSetRecord, CredentialSetId> credentialDataSetRepository,
    IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId> mdocCredentialRepository,
    IDomainRepository<SdJwtCredential, SdJwtCredentialRecord, CredentialId> sdJwtCredentialRepository,
    IHttpClientFactory httpClientFactory,
    IIssuerMetadataService issuerMetadataService,
    ITokenService tokenService) : IOid4VciClientService
{
    private const string AuthorizationCodeGrantTypeIdentifier = "authorization_code";
    private const string PreAuthorizedCodeGrantTypeIdentifier = "urn:ietf:params:oauth:grant-type:pre-authorized_code";

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    /// <inheritdoc />
    public async Task<Uri> InitiateAuthFlow(
        CredentialOfferMetadata offer,
        ClientOptions clientOptions,
        Option<ClientAttestationDetails> clientAttestationDetails)
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
                _ => new AuthorizationDetails(
                    pair.Key.ToString(),
                    issuerMetadata.AuthorizationServers.ToNullable()?.Select(id => id.ToString()).ToArray()
                ),
                _ => new AuthorizationDetails(
                    pair.Key.ToString(),
                    issuerMetadata.AuthorizationServers.ToNullable()?.Select(id => id.ToString()).ToArray())
                )
            );

        var authCode =
            from grants in offer.CredentialOffer.Grants
            from code in grants.AuthorizationCode
            select code;

        var issuerState =
            from code in authCode
            from issState in code.IssuerState
            select issState;

        var vciAuthorizationRequest = new VciAuthorizationRequest(
            sessionId,
            clientOptions,
            authorizationCodeParameters,
            authorizationDetails.ToArray(),
            scope,
            issuerState.ToNullable(),
            null,
            null);
        
        var authServerMetadata = await FetchAuthorizationServerMetadataAsync(issuerMetadata, offer.CredentialOffer);
        
        var authorizationRequestUri = authServerMetadata.PushedAuthorizationRequestEndpoint.IsNullOrEmpty()
            ? new Uri(authServerMetadata.AuthorizationEndpoint + vciAuthorizationRequest.ToQueryString())
            : await GetRequestUriUsingPushedAuthorizationRequest(authServerMetadata, vciAuthorizationRequest, clientAttestationDetails);
        
        var authorizationData = new AuthorizationData(
            clientOptions,
            issuerMetadata,
            authServerMetadata,
            Option<OAuthToken>.None, 
            offer.CredentialOffer.CredentialConfigurationIds);

        var session = new AuthFlowSession(
            sessionId,
            authorizationData,
            authorizationCodeParameters,
            Option<int>.None);
        
        await authFlowSessionRepository.Add(session);
            
        return authorizationRequestUri;
    }

    public async Task<Uri> InitiateAuthFlow(Uri uri, ClientOptions clientOptions, Option<ClientAttestationDetails> clientAttestationDetails, Option<Locale> language, Option<OneOf<Vct, DocType>> credentialType, Option<int> specVersion)
    {
        var locale = language.Match(
            some => some,
            () => Core.Localization.Constants.DefaultLocale);
        
        var issuerMetadata = issuerMetadataService.ProcessMetadata(uri, locale);
        
        return await issuerMetadata.Match(
            async validIssuerMetadata =>
            {
                var authServerMetadata = 
                    await FetchAuthorizationServerMetadataAsync(validIssuerMetadata, Option<CredentialOffer>.None);

                var sessionId = AuthFlowSessionState.CreateAuthFlowSessionState();
                var authorizationCodeParameters = CreateAndStoreCodeChallenge();

                var relevantConfigurations = validIssuerMetadata.CredentialConfigurationsSupported
                    .Where(config =>
                    {
                        return credentialType.Match(
                            type => config.Value.Match(
                                sdJwtConfig => type.IsT0 && sdJwtConfig.Vct ==  type.AsT0,
                                mDocConfig => type.IsT1 && mDocConfig.DocType == type.AsT1),
                            () => true);
                    }).ToList();
                
                var scopes = relevantConfigurations
                    .Select(config => config.Value.Match(
                        sdJwtConfig => sdJwtConfig.CredentialConfiguration.Scope.OnSome(scope => scope.ToString()),
                        mdDocConfig => mdDocConfig.CredentialConfiguration.Scope.OnSome(scope => scope.ToString())
                    ))
                    .Where(option => option.IsSome)
                    .Select(option => option.Fallback(string.Empty))
                    .Distinct();
                
                var authorizationDetails = relevantConfigurations
                    .Select(config => new AuthorizationDetails(
                        config.Key.ToString(),
                        validIssuerMetadata.AuthorizationServers.ToNullable()?.Select(id => id.ToString()).ToArray()
                    )).ToArray();
                
                var vciAuthorizationRequest = new VciAuthorizationRequest(
                    sessionId,
                    clientOptions,
                    authorizationCodeParameters,
                    authorizationDetails, 
                    string.Join(" ", scopes),
                    null,
                    null,
                    null);
                
                var authorizationRequestUri = authServerMetadata.PushedAuthorizationRequestEndpoint.IsNullOrEmpty()
                    ? new Uri(authServerMetadata.AuthorizationEndpoint + vciAuthorizationRequest.ToQueryString())
                    : await GetRequestUriUsingPushedAuthorizationRequest(authServerMetadata, vciAuthorizationRequest, clientAttestationDetails);
                
                //TODO: Select multiple configurationIds
                var authorizationData = new AuthorizationData(
                    clientOptions,
                    validIssuerMetadata,
                    authServerMetadata,
                    Option<OAuthToken>.None,
                    relevantConfigurations
                        .Select(config => config.Key)
                        .ToList());

                var session = new AuthFlowSession(
                    sessionId,
                    authorizationData,
                    authorizationCodeParameters,
                    specVersion);
                await authFlowSessionRepository.Add(session);
            
                return authorizationRequestUri;
            },
            _ => throw new Exception("Fetching Issuer metadata failed")
            );
    }

    private async Task<Uri> GetRequestUriUsingPushedAuthorizationRequest(AuthorizationServerMetadata authorizationServerMetadata, VciAuthorizationRequest vciAuthorizationRequest, Option<ClientAttestationDetails> clientAttestationDetails)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        
        await clientAttestationDetails.IfSomeAsync(async attestationDetails =>
        {
            var combinedWalletAttestation = await clientAttestationService.GetCombinedWalletAttestationAsync(attestationDetails, authorizationServerMetadata);
            _httpClient.AddClientAttestationPopHeader(combinedWalletAttestation);
        });
        
        var response = await _httpClient.PostAsync(
            authorizationServerMetadata.PushedAuthorizationRequestEndpoint,
            vciAuthorizationRequest.ToFormUrlEncoded()
        );

        var parResponse = DeserializeObject<PushedAuthorizationRequestResponse>(await response.Content.ReadAsStringAsync()) 
                          ?? throw new InvalidOperationException("Failed to deserialize the PAR response.");
            
        return new Uri(authorizationServerMetadata.AuthorizationEndpoint 
                       + "?client_id=" + vciAuthorizationRequest.ClientId 
                       + "&request_uri=" + System.Net.WebUtility.UrlEncode(parResponse.RequestUri.ToString()));
    }
    
    public async Task<Validation<IEnumerable<CredentialDataSet>>> AcceptOffer(
        CredentialOfferMetadata credentialOfferMetadata,
        Option<ClientAttestationDetails> clientAttestationDetails,
        string? transactionCode)
    {
        var issuerMetadata = credentialOfferMetadata.IssuerMetadata;
        
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

        var authorizationServerMetadata = await FetchAuthorizationServerMetadataAsync(issuerMetadata, credentialOfferMetadata.CredentialOffer);
        
        var token = await tokenService.RequestToken(
            tokenRequest,
            authorizationServerMetadata,
            clientAttestationDetails,
            issuerMetadata.CredentialNonceEndpoint);

        // TODO: Support multiple configs
        var configurationId = credentialOfferMetadata.CredentialOffer.CredentialConfigurationIds.First();
        var configurationPair = issuerMetadata.CredentialConfigurationsSupported.Single(config => config.Key == configurationId);
        
        var validResponses = await credentialRequestService.RequestCredentials(
            configurationPair,
            issuerMetadata,
            token,
            Option<ClientOptions>.None,
            Option<AuthorizationRequest>.None,
            Option<int>.None);

        var credentialSets = new List<CredentialDataSet>();
        var result =
            from responses in validResponses
            let setId = CredentialSetId.CreateCredentialSetId()
            select
                from response in responses
                let credentialsOrTransactionId = response.CredentialsOrTransactionId
                select credentialsOrTransactionId.Match(
                    async creds =>
                    {
                        var records = new List<ICredential>();
                        foreach (var credential in creds)
                        {
                            await credential.Value.Match(
                                async sdJwt =>
                                {
                                    var record = sdJwt.Decoded.ToCredential(
                                        configurationPair.Value.AsT0,
                                        response.KeyId,
                                        setId,
                                        creds.Count > 1);
                                    
                                    await sdJwtCredentialRepository.Add(record);
                                    records.Add(record);
                                },
                                async mdoc =>
                                {
                                    var displays = MdocFun.CreateMdocDisplays(configurationPair.Value.AsT1);
                                    
                                    var mdocCredential = new MdocCredential(
                                        mdoc.Decoded,
                                        CredentialId.CreateCredentialId(), 
                                        setId,
                                        displays,
                                        response.KeyId,
                                        CredentialState.Active,
                                        creds.Count > 1,
                                        Option<DateTime>.None);
                                    
                                    await mdocCredentialRepository.Add(mdocCredential);
                                    records.Add(mdocCredential);
                                });
                        }
                        
                        var dataSet = CredentialDataSet.FromCredentials(
                            records,
                            issuerMetadata.CredentialIssuer.ToString());
                        
                        credentialSets.Add(dataSet);
                    },
                    // ReSharper disable once UnusedParameter.Local
                    transactionId => throw new NotImplementedException());

        await result.OnSuccess(async tasks => await Task.WhenAll(tasks));
        
        await credentialDataSetRepository.AddMany(credentialSets);
        
        return credentialSets;
    }

    public async Task<Validation<CredentialOfferMetadata>> ProcessOffer(Uri credentialOffer, Option<Locale> language)
    {
        var locale = language.Match(
            some => some,
            () => Core.Localization.Constants.DefaultLocale);
        
        var result =
            from offer in credentialOfferService.ProcessCredentialOffer(credentialOffer, locale)
            from metadata in issuerMetadataService.ProcessMetadata(offer.CredentialIssuer, locale)
            select new CredentialOfferMetadata(offer, metadata);

        return await result;
    }

    /// <inheritdoc />
    public async Task<Validation<IEnumerable<CredentialDataSet>>> RequestCredentialSet(
        IssuanceSession issuanceSession,
        Option<ClientAttestationDetails> clientAttestationDetails)
    {
        var session = (await authFlowSessionRepository.GetById(issuanceSession.AuthFlowSessionState))
            .UnwrapOrThrow(new InvalidOperationException("Auth flow session not found"));
        
        var relevantConfigurations = session
            .AuthorizationData
            .IssuerMetadata
            .CredentialConfigurationsSupported
            .Where(config => session.AuthorizationData.CredentialConfigurationIds.Contains(config.Key));

        var configurations = relevantConfigurations.ToList();
        
        var scopes = configurations
            .Select(config => config.Value.Match(
                sdJwtConfig => sdJwtConfig.CredentialConfiguration.Scope.OnSome(scope => scope.ToString()),
                mdDocConfig => mdDocConfig.CredentialConfiguration.Scope.OnSome(scope => scope.ToString())))
            .Where(scope => scope.IsSome)
            .Select(option => option.Fallback(string.Empty));
        
        var tokenRequest = new TokenRequest
        {
            GrantType = AuthorizationCodeGrantTypeIdentifier,
            RedirectUri = session.AuthorizationData.ClientOptions.RedirectUri,
            CodeVerifier = session.AuthorizationCodeParameters.Verifier,
            Code = issuanceSession.Code,
            Scope = string.Join(" ", scopes),
            ClientId = session.AuthorizationData.ClientOptions.ClientId
        };
        
        var token = await tokenService.RequestToken(
            tokenRequest,
            session.AuthorizationData.AuthorizationServerMetadata,
            clientAttestationDetails,
            session.AuthorizationData.IssuerMetadata.CredentialNonceEndpoint);
        
        //TODO: Make sure that it does not always request all available credConfigurations
        var credentialSets = new List<CredentialDataSet>();
        foreach (var configuration in configurations)
        {
            var validResponses = await credentialRequestService.RequestCredentials(
                configuration,
                session.AuthorizationData.IssuerMetadata,
                token,
                session.AuthorizationData.ClientOptions,
                Option<AuthorizationRequest>.None,
                session.SpecVersion.ToOption());
            
            var result =
                from responses in validResponses
                let setId = CredentialSetId.CreateCredentialSetId()
                select
                    from response in responses
                    let cNonce = response.CNonce
                    let credentialsOrTransactionId = response.CredentialsOrTransactionId
                    select credentialsOrTransactionId.Match(
                        async creds =>
                        {
                            token = await session.AuthorizationData.IssuerMetadata.CredentialNonceEndpoint.Match(
                                Some: async credentialNonceEndpoint =>
                                {
                                    var credentialNonce = await credentialNonceService.GetCredentialNonce(credentialNonceEndpoint);
                                    return token.Match<OneOf<OAuthToken, DPopToken>>(
                                        oAuth => oAuth with { CNonce = credentialNonce.Value },
                                        dPop => dPop with
                                        {
                                            Token = dPop.Token with { CNonce = credentialNonce.Value }
                                        });
                                },
                                None: () =>
                                {
                                    return Task.FromResult( token.Match<OneOf<OAuthToken, DPopToken>>(
                                        oAuth => oAuth with { CNonce = cNonce.ToNullable() },
                                        dPop => dPop with
                                        {
                                            Token = dPop.Token with { CNonce = cNonce.ToNullable() }
                                        }));
                                });
                            
                            var records = new List<ICredential>();
                            foreach (var credential in creds)
                            {
                                await credential.Value.Match(
                                    async sdJwt =>
                                    {
                                        var record = sdJwt.Decoded.ToCredential(
                                            configuration.Value.AsT0,
                                            response.KeyId,
                                            setId,
                                            creds.Count > 1);
                                        
                                        await sdJwtCredentialRepository.Add(record);
                                        records.Add(record);
                                    },
                                    async mdoc =>
                                    {
                                        var displays = MdocFun.CreateMdocDisplays(configuration.Value.AsT1);
                                        
                                        var mdocCredential = new MdocCredential(
                                            mdoc.Decoded,
                                            CredentialId.CreateCredentialId(),
                                            setId,
                                            displays,
                                            response.KeyId,
                                            CredentialState.Active,
                                            creds.Count > 1,
                                            Option<DateTime>.None);
                                        
                                        await mdocCredentialRepository.Add(mdocCredential);
                                        records.Add(mdocCredential);
                                    });   
                            }
                            var dataSet = CredentialDataSet.FromCredentials(
                                records,
                                session.AuthorizationData.IssuerMetadata.CredentialIssuer.ToString());
                            credentialSets.Add(dataSet);
                        },
                        // ReSharper disable once UnusedParameter.Local
                        transactionId => throw new NotImplementedException());

            await result.OnSuccess(async tasks => await Task.WhenAll(tasks));
        }

        await credentialDataSetRepository.AddMany(credentialSets);
        
        await authFlowSessionRepository.Delete(session.AuthFlowSessionState);
        
        return credentialSets;
    }
    
    //TODO: Refactor this C'' method into current flows (too much duplicate code)
    /// <inheritdoc />
    public async Task<Validation<IEnumerable<OnDemandCredentialSet>>> RequestOnDemandCredentialSet(IssuanceSession issuanceSession, AuthorizationRequest authorizationRequest, Option<ClientAttestationDetails> clientAttestationDetails)
    {
        var session = (await authFlowSessionRepository.GetById(issuanceSession.AuthFlowSessionState))
            .UnwrapOrThrow(new InvalidOperationException("Auth flow session not found"));
        
        var relevantConfigurations = session
            .AuthorizationData
            .IssuerMetadata
            .CredentialConfigurationsSupported
            .Where(config => session.AuthorizationData.CredentialConfigurationIds.Contains(config.Key));

        var configurations = relevantConfigurations.ToList();
        var scopes = configurations
            .Select(config => config.Value.Match(
                sdJwtConfig => sdJwtConfig.CredentialConfiguration.Scope.OnSome(scope => scope.ToString()),
                mdDocConfig => mdDocConfig.CredentialConfiguration.Scope.OnSome(scope => scope.ToString())))
            .Where(scope => scope.IsSome)
            .Select(option => option.Fallback(string.Empty));
        
        var tokenRequest = new TokenRequest
        {
            GrantType = AuthorizationCodeGrantTypeIdentifier,
            RedirectUri = session.AuthorizationData.ClientOptions.RedirectUri,
            CodeVerifier = session.AuthorizationCodeParameters.Verifier,
            Code = issuanceSession.Code,
            Scope = string.Join(" ", scopes),
            ClientId = session.AuthorizationData.ClientOptions.ClientId
        };
        
        var token = await tokenService.RequestToken(
            tokenRequest,
            session.AuthorizationData.AuthorizationServerMetadata,
            clientAttestationDetails,
            session.AuthorizationData.IssuerMetadata.CredentialNonceEndpoint);

        var credentials = new List<(CredentialDataSet, List<ICredential>)>();
        
        //TODO: Make sure that it does not always request all available credConfigurations
        foreach (var configuration in configurations)
        {
            var validResponses = await credentialRequestService.RequestCredentials(
                configuration,
                session.AuthorizationData.IssuerMetadata,
                token,
                session.AuthorizationData.ClientOptions,
                authorizationRequest,
                session.SpecVersion.ToOption());
            
            var result =
                from responses in validResponses
                let setId = CredentialSetId.CreateCredentialSetId()
                select
                    from response in responses
                    let cNonce = response.CNonce
                    let credentialsOrTransactionId = response.CredentialsOrTransactionId
                    select credentialsOrTransactionId.Match(
                        async creds =>
                        {
                            token = await session.AuthorizationData.IssuerMetadata.CredentialNonceEndpoint.Match(
                                Some: async credentialNonceEndpoint =>
                                {
                                    var credentialNonce = await credentialNonceService.GetCredentialNonce(credentialNonceEndpoint);
                                    return token.Match<OneOf<OAuthToken, DPopToken>>(
                                        oAuth =>
                                        {
                                            var newAuthData = session.AuthorizationData with { OAuthToken = oAuth };
                                            session = session with { AuthorizationData = newAuthData };
                                            
                                            return oAuth with
                                            {
                                                CNonce = credentialNonce.Value
                                            };
                                        },
                                        dPop =>
                                        {
                                            var newAuthData2 = session.AuthorizationData with { OAuthToken = dPop.Token };
                                            session = session with { AuthorizationData = newAuthData2 };
                                            
                                            return dPop with
                                            {
                                                Token = dPop.Token with { CNonce = credentialNonce.Value }
                                            };
                                        });
                                },
                                None: () =>
                                {
                                    return Task.FromResult(token.Match<OneOf<OAuthToken, DPopToken>>(
                                        oAuth =>
                                        {
                                            var newAuthData3 = session.AuthorizationData with { OAuthToken = oAuth };
                                            session = session with { AuthorizationData = newAuthData3 };
                                            
                                            return oAuth with { CNonce = cNonce.ToNullable() };
                                        },
                                        dPop =>
                                        {
                                            var newAuthData4 = session.AuthorizationData with { OAuthToken = dPop.Token };
                                            session = session with { AuthorizationData = newAuthData4 };
                                            
                                            return dPop with
                                            {
                                                Token = dPop.Token with { CNonce = cNonce.ToNullable() }
                                            };
                                        }));
                                });
                            
                            var records = new List<ICredential>();
                            foreach (var credential in creds)
                            {
                                var record = credential.Value.Match<ICredential>(
                                sdJwt =>
                                {
                                    var record = sdJwt.Decoded.ToCredential(
                                        configuration.Value.AsT0,
                                        response.KeyId,
                                        setId,
                                        creds.Count > 1);
                                    records.Add(record);
                                    return record;
                                },
                                mdoc =>
                                {
                                    var displays = MdocFun.CreateMdocDisplays(configuration.Value.AsT1);
                                    
                                    var mdocCredential = new MdocCredential(
                                        mdoc.Decoded,
                                        CredentialId.CreateCredentialId(),
                                        setId,
                                        displays,
                                        response.KeyId,
                                        CredentialState.Active,
                                        creds.Count > 1,
                                        Option<DateTime>.None);
                                    records.Add(mdocCredential);
                                    return mdocCredential;
                                });
                                
                                records.Add(record);
                            }
                            
                            var dataSet = CredentialDataSet.FromCredentials(
                                records,
                                session.AuthorizationData.IssuerMetadata.CredentialIssuer.ToString());
                            credentials.Add((dataSet, records));
                        },
                        // ReSharper disable once UnusedParameter.Local
                        transactionId => throw new NotImplementedException());

            await result.OnSuccess(async tasks => await Task.WhenAll(tasks));
        }
        
        await authFlowSessionRepository.Update(session);

        return credentials.Select(credential => new OnDemandCredentialSet(credential.Item1, credential.Item2)).ToList();
    }

    private static AuthorizationCodeParameters CreateAndStoreCodeChallenge()
    {
        var randomNumber = RandomNumberGenerator.GetBytes(32);
        var codeVerifier = Base64UrlEncoder.Encode(randomNumber);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));
        var codeChallenge = Base64UrlEncoder.Encode(bytes);

        return new AuthorizationCodeParameters(codeChallenge, codeVerifier);
    }

    private async Task<AuthorizationServerMetadata> FetchAuthorizationServerMetadataAsync(IssuerMetadata issuerMetadata, Option<CredentialOffer> credentialOffer)
    {
        Uri credentialIssuer = issuerMetadata.CredentialIssuer;
        
        var authServerUrls = issuerMetadata.AuthorizationServers.Match(
            issuerMetadataAuthServers =>
            {
                var credentialOfferAuthServer = from offer in credentialOffer
                    from grants in offer.Grants
                    from code in grants.AuthorizationCode
                    from server in code.AuthorizationServer
                    select server;
                
                return credentialOfferAuthServer.Match(
                    offerAuthServer =>
                    {
                        var matchingAuthServer = issuerMetadataAuthServers.Find(issuerMetadataAuthServer => issuerMetadataAuthServer.ToString() == offerAuthServer);
        
                        return matchingAuthServer.Match(
                            Some: server => new List<Uri>(){CreateAuthorizationServerMetadataUri(server)},
                            None: () => throw new InvalidOperationException(
                                "The authorization server in the credential offer does not match any authorization server in the issuer metadata."));
                    },
                    () => issuerMetadataAuthServers.Select(uri => CreateAuthorizationServerMetadataUri(uri))
                    );
            },
            () => new List<Uri>(){CreateAuthorizationServerMetadataUri(credentialIssuer)});


        var authorizationServerMetadatas = new List<AuthorizationServerMetadata>();
        foreach (var authServerUrl in authServerUrls)
        {
            var getAuthServerResponse = await _httpClient.GetAsync(authServerUrl);
            
            if (!getAuthServerResponse.IsSuccessStatusCode)
                continue;
            
            var content = await getAuthServerResponse.Content.ReadAsStringAsync();
        
            var authServer = DeserializeObject<AuthorizationServerMetadata>(content)
                             ?? throw new InvalidOperationException(
                                 "Failed to deserialize the authorization server metadata.");
            
            authorizationServerMetadatas.Add(authServer);
        }

        return credentialOffer.Match(
            Some: offer =>
            {
                var credentialOfferAuthCodeGrantType = from grants in offer.Grants 
                    from code in grants.AuthorizationCode
                    select code;

                return  credentialOfferAuthCodeGrantType.Match(
                    Some: code => code.AuthorizationServer.Match(
                        Some: requestedAuthServer => 
                            authorizationServerMetadatas.Find(authServer => 
                                authServer.Issuer == requestedAuthServer.ToString())
                            ?? throw new InvalidOperationException("No suitable Authorization Server found"),
                        None: () => authorizationServerMetadatas.Find(authServer => authServer.SupportsAuthCodeFlow)
                                    ?? authorizationServerMetadatas.FirstOrDefault()
                                    ?? throw new InvalidOperationException("No suitable Authorization Server found")),
                    None: () =>
                    {
                        var credentialOfferPreAuthGrantType = from grants in offer.Grants 
                            from code in grants.PreAuthorizedCode
                            select code;

                        return credentialOfferPreAuthGrantType.Match(
                            Some: preAuth =>
                            {
                                return preAuth.AuthorizationServer.Match(
                                    Some: requestedAuthServer => 
                                        authorizationServerMetadatas.Find(authServer =>
                                            authServer.Issuer == requestedAuthServer.ToString()) 
                                        ?? throw new InvalidOperationException("No suitable Authorization Server found"),
                                    None: () => authorizationServerMetadatas.Find(authServer => authServer.SupportsPreAuthFlow) 
                                                ?? authorizationServerMetadatas.FirstOrDefault()
                                                ?? throw new InvalidOperationException("No suitable Authorization Server found"));
                            },
                            None: () => authorizationServerMetadatas.First());
                    });
            },
            None: () => authorizationServerMetadatas.Find(authServer => authServer.SupportsAuthCodeFlow)
                        ?? authorizationServerMetadatas.FirstOrDefault()
                        ?? throw new InvalidOperationException("No suitable Authorization Server found"));
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
