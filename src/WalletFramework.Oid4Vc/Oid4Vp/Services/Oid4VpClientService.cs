using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using OneOf;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Functional;
using WalletFramework.Core.String;
using WalletFramework.MdocLib;
using WalletFramework.MdocLib.Device;
using WalletFramework.MdocLib.Device.Response;
using WalletFramework.MdocLib.Elements;
using WalletFramework.MdocLib.Security;
using WalletFramework.MdocLib.Security.Cose;
using WalletFramework.MdocVc;
using WalletFramework.MdocVc.Persistence;
using WalletFramework.Oid4Vc.ClientAttestation;
using WalletFramework.Oid4Vc.Errors;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Persistence;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Extensions;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Errors;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Persistence;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;
using WalletFramework.Oid4Vc.Qes.Authorization;
using WalletFramework.SdJwtLib.Models;
using WalletFramework.SdJwtVc;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.SdJwtVc.Persistence;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;
using WalletFramework.Storage;
using static Newtonsoft.Json.JsonConvert;
using static WalletFramework.MdocLib.Security.Cose.ProtectedHeaders;
using Format = WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Format;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

/// <inheritdoc />
public class Oid4VpClientService : IOid4VpClientService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Oid4VpClientService" /> class.
    /// </summary>
    /// <param name="authFlowSessionStorage">The Auth Flow Session Storage.</param>
    /// <param name="authorizationRequestService">The authorization request service.</param>
    /// <param name="authorizationResponseEncryptionService">The authorization response encryption service.</param>
    /// <param name="candidateQueryService">The Presentation Candidate service.</param>
    /// <param name="clientAttestationService">The client attestation service.</param>
    /// <param name="httpClientFactory">The http client factory to create http clients.</param>
    /// <param name="logger">The ILogger.</param>
    /// <param name="mdocRepository">The service responsible for mdoc storage operations.</param>
    /// <param name="sdJwtRepository">The service responsible for SD-JWT storage operations.</param>
    /// <param name="oid4VpHaipClient">The service responsible for OpenId4VP related operations.</param>
    /// <param name="presentationRepository">The service responsible for OidPresentationRecord related operations.</param>
    /// <param name="presentationService">The authorization response service.</param>
    /// <param name="sdJwtVcHolderService">The service responsible for SD-JWT related operations.</param>
    public Oid4VpClientService(
        IDomainRepository<AuthFlowSession, AuthFlowSessionRecord, AuthFlowSessionState> authFlowSessionStorage,
        IAuthorizationRequestService authorizationRequestService,
        IAuthorizationResponseEncryptionService authorizationResponseEncryptionService,
        ICandidateQueryService candidateQueryService,
        IClientAttestationService clientAttestationService,
        IHttpClientFactory httpClientFactory,
        ILogger<Oid4VpClientService> logger,
        IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId> mdocRepository,
        IDomainRepository<SdJwtCredential, SdJwtCredentialRecord, CredentialId> sdJwtRepository,
        IDomainRepository<CompletedPresentation, CompletedPresentationRecord, string> presentationRepository, 
        IOid4VpHaipClient oid4VpHaipClient,
        IPresentationService presentationService,
        IVerifierKeyService verifierKeyService,
        ISdJwtVcHolderService sdJwtVcHolderService)
    {
        _authFlowSessionStorage = authFlowSessionStorage;
        _authorizationRequestService = authorizationRequestService;
        _authorizationResponseEncryptionService = authorizationResponseEncryptionService;
        _candidateQueryService = candidateQueryService;
        _clientAttestationService = clientAttestationService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _mdocRepository = mdocRepository;
        _sdJwtRepository = sdJwtRepository;
        _oid4VpHaipClient = oid4VpHaipClient;
        _presentationRepository = presentationRepository;
        _presentationService = presentationService;
        _verifierKeyService = verifierKeyService;
        _sdJwtVcHolderService = sdJwtVcHolderService;
    }

    private readonly IDomainRepository<AuthFlowSession, AuthFlowSessionRecord, AuthFlowSessionState> _authFlowSessionStorage;
    private readonly IAuthorizationRequestService _authorizationRequestService;
    private readonly IAuthorizationResponseEncryptionService _authorizationResponseEncryptionService;
    private readonly ICandidateQueryService _candidateQueryService;
    private readonly IClientAttestationService _clientAttestationService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Oid4VpClientService> _logger;
    private readonly IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId> _mdocRepository;
    private readonly IDomainRepository<SdJwtCredential, SdJwtCredentialRecord, CredentialId> _sdJwtRepository;
    private readonly IDomainRepository<CompletedPresentation, CompletedPresentationRecord, string> _presentationRepository;
    private readonly IOid4VpHaipClient _oid4VpHaipClient;
    private readonly IPresentationService _presentationService;
    private readonly IVerifierKeyService _verifierKeyService;
    private readonly ISdJwtVcHolderService _sdJwtVcHolderService;

    public async Task<Option<Uri>> AbortAuthorizationRequest(AuthorizationRequestCancellation cancellation)
    {
        var callbackTaskOption = cancellation.ResponseUri.OnSome(
            async uri =>
            {
                var error = cancellation.Errors.First();

                var message = new HttpRequestMessage(HttpMethod.Post, uri)
                {
                    Content = error.ToResponse().ToFormUrlContent()
                };

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Clear();

                using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                var responseMessage = await httpClient.SendAsync(message, cancellationSource.Token);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    var str = await responseMessage.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"Authorization Error Response failed with message {str}");
                }

                var redirectUriJson = await responseMessage.Content.ReadAsStringAsync();
                var callback = DeserializeObject<AuthorizationResponseCallback>(redirectUriJson);
                return callback?.ToUri() ?? Option<Uri>.None;
            });

        var callbackUriOption = await callbackTaskOption.Traverse(uri => uri);
        return callbackUriOption.Flatten();
    }

    public async Task<Option<Uri>> AcceptAuthorizationRequest(
        AuthorizationRequest authorizationRequest,
        IEnumerable<SelectedCredential> selectedCredentials,
        Option<ClientAttestationDetails> clientAttestationDetails)
    {
        var credentials = selectedCredentials.ToList();

        var (presentations, mdocNonce) = await _presentationService.CreatePresentations(
            authorizationRequest, 
            credentials);

        var authorizationResponse = await _oid4VpHaipClient.CreateAuthorizationResponseAsync(
            authorizationRequest,
            presentations.Select(tuple => tuple.PresentationMap).ToArray()
        );

        var content = authorizationRequest.ResponseMode switch
        {
            AuthorizationRequest.DirectPost => authorizationResponse.ToFormUrl(),
            AuthorizationRequest.DirectPostJwt => 
                (await _authorizationResponseEncryptionService.Encrypt(authorizationResponse, authorizationRequest, mdocNonce)).ToFormUrl(),
            _ => throw new ArgumentOutOfRangeException(nameof(authorizationRequest.ResponseMode))
        };

        var message = new HttpRequestMessage
        {
            RequestUri = new Uri(authorizationRequest.ResponseUri),
            Method = HttpMethod.Post,
            Content = content
        };

        // TODO: Introduce timeout
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Clear();
        
        await clientAttestationDetails.IfSomeAsync(async details =>
        {
            var combinedWalletAttestation = await _clientAttestationService.GetCombinedWalletAttestationAsync(details, authorizationRequest);
            httpClient.AddClientAttestationPopHeader(combinedWalletAttestation);
        });

        // ToDo: when to delete these records?
        foreach (var credential in credentials)
        {
            switch (credential.Credential)
            {
                case SdJwtCredential { OneTimeUse: true } sdJwtRecord:
                    var credentialSetSdJwtRecords = await _sdJwtRepository.ListAll();
                    await credentialSetSdJwtRecords.Match(
                        async sdJwtRecords =>
                        {
                            if (sdJwtRecords.Count() > 1)
                                await _sdJwtRepository.Delete(sdJwtRecord.GetId());
                        },
                        () => Task.CompletedTask);
                    break;
                case MdocCredential { OneTimeUse: true } mDocRecord:
                    var credentialSetMdocRecords = await _mdocRepository.ListAll();
                    await credentialSetMdocRecords.Match(
                        async mDocRecords =>
                        {
                            if (mDocRecords.Count() > 1)
                                await _mdocRepository.Delete(mDocRecord.GetId());
                        },
                        () => Task.CompletedTask);
                    break;
            }
        }

        var responseMessage = await httpClient.SendAsync(message);
        if (!responseMessage.IsSuccessStatusCode)
        {
            var str = await responseMessage.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Authorization Response failed with message {str}");
        }

        var presentedCredentials = presentations.Select(presentation =>
        {
            PresentedCredentialSet result;

            switch (presentation.PresentedCredential)
            {
                case SdJwtCredential sdJwtRecord:
                    var issuanceSdJwtDoc = sdJwtRecord.ToSdJwtDoc();
                    var sdJwtDoc = new SdJwtDoc(presentation.PresentationMap.Presentation);

                    var nonDisclosed =
                        from disclosure in issuanceSdJwtDoc.Disclosures
                        let base64Encoded = disclosure.Base64UrlEncoded
                        where sdJwtDoc.Disclosures.All(itm => itm.Base64UrlEncoded != base64Encoded)
                        select disclosure;

                    var presentedClaims =
                        from claim in sdJwtRecord.Claims
                        where !nonDisclosed.Any(nd => claim.Key.StartsWith(nd.Path ?? string.Empty))
                        select new
                        {
                            key = claim.Key,
                            value = new PresentedClaim { Value = claim.Value }
                        };
                    
                    result = new PresentedCredentialSet
                    {
                        SdJwtCredentialType = Vct.ValidVct(sdJwtRecord.Vct).UnwrapOrThrow(),
                        CredentialSetId = CredentialSetId.ValidCredentialSetId(sdJwtRecord.CredentialSetId)
                            .UnwrapOrThrow(),
                        PresentedClaims = presentedClaims.ToDictionary(itm => itm.key, itm => itm.value)
                    };
                    break;
                case MdocCredential mdocCredential:
                    var claims = mdocCredential.Mdoc.IssuerSigned.IssuerNameSpaces.Value.SelectMany(pair => pair.Value);

                    result = new PresentedCredentialSet
                    {
                        MdocCredentialType = mdocCredential.Mdoc.DocType,
                        CredentialSetId = mdocCredential.GetCredentialSetId(),
                        PresentedClaims = claims.ToDictionary(
                            item => item.ElementId.ToString(),
                            item => new PresentedClaim { Value = item.Element.ToString() }
                        )
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(presentation.PresentedCredential));
            }

            return result;
        });

        var oidPresentationRecord = new CompletedPresentation(
            Guid.NewGuid().ToString(),
            authorizationRequest.ClientId!,
            presentedCredentials.ToList(),
            authorizationRequest.ClientMetadata,
            authorizationRequest.Requirements.Match(
                _ => Option<string>.None,
                presentationDefinition => presentationDefinition.Name),
            DateTime.UtcNow);

        await _presentationRepository.Add(oidPresentationRecord);

        var redirectUriJson = await responseMessage.Content.ReadAsStringAsync();

        try
        {
            Uri callback = DeserializeObject<AuthorizationResponseCallback>(redirectUriJson);
            return callback;
        }
        catch (Exception e)
        {
            _logger.LogWarning(
                e,
                "Could not parse Redirect URI received from: {ResponseUri}", authorizationRequest.ResponseUri);

            return null;
        }
    }

    //TODO: Refactor this C'' method into current flows (too much duplicate code)
    public async Task<Option<Uri>> AcceptOnDemandRequest(
        AuthorizationRequest authorizationRequest,
        IEnumerable<SelectedCredential> selectedCredentials,
        IssuanceSession issuanceSession)
    {
        var credentials = selectedCredentials.ToList();

        var mdocNonce = Option<Nonce>.None;
        
        var presentations = new List<(PresentationMap PresentationMap, ICredential PresentedCredential)>();
        foreach (var credential in credentials)
        {
            var credentialRequirement =
                authorizationRequest.Requirements.Match<OneOf<CredentialQuery, InputDescriptor>>(
                    dcqlQuery =>
                        dcqlQuery.CredentialQueries.Single(credentialQuery =>
                            credentialQuery.Id == credential.Identifier),
                    presentationDefinition =>
                        presentationDefinition.InputDescriptors.Single(inputDescriptor =>
                            inputDescriptor.Id == credential.Identifier));

            var credentialRequirementId = credentialRequirement.Match(
                credentialQuery => credentialQuery.Id,
                inputDescriptor => inputDescriptor.Id);

            var claims = credentialRequirement.Match(
                credentialQuery => credential.GetClaimsToDiscloseAsStrs(credentialQuery),
                inputDescriptor => inputDescriptor.GetRequestedAttributes());

            Format format;
            ICredential presentedCredential;

            var session = (await _authFlowSessionStorage.GetById(issuanceSession.AuthFlowSessionState))
                .UnwrapOrThrow(new InvalidOperationException("Auth flow session not found"));

            var client = _httpClientFactory.CreateClient();
            client.WithAuthorizationHeader(session.AuthorizationData.OAuthToken.UnwrapOrThrow(new Exception()));

            var sha256 = SHA256.Create();

            var presentation = string.Empty;
            switch (credential.Credential)
            {
                case SdJwtCredential sdJwt:
                    format = Format.ValidFormat(sdJwt.Format).UnwrapOrThrow();

                    presentation = await _sdJwtVcHolderService.CreatePresentation(
                        sdJwt,
                        claims.ToArray(),
                        Option<IEnumerable<string>>.None,
                        Option<IEnumerable<string>>.None,
                        Option<string>.None,
                        authorizationRequest.ClientId,
                        authorizationRequest.Nonce);

                    var kbJwt = presentation[presentation.LastIndexOf('~')..][1..];
                    var kbJwtWithoutSignature = kbJwt[..kbJwt.LastIndexOf('.')];

                    var kbJwtWithoutSignatureHash = sha256.ComputeHash(kbJwtWithoutSignature.GetUtf8Bytes());

                    var sdJwtContent = new JObject
                    {
                        {
                            "hash_bytes",
                            Base64UrlEncoder.Encode(kbJwtWithoutSignatureHash)
                        }
                    };

                    var sdJwtHttpContent = new StringContent(
                        sdJwtContent.ToString(),
                        Encoding.UTF8,
                        MediaTypeNames.Application.Json);

                    var sdJwtSignatureResponse = await client.PostAsync(
                        session.AuthorizationData.IssuerMetadata.PresentationSigningEndpoint.UnwrapOrThrow(
                            new Exception()), sdJwtHttpContent
                    );

                    if (sdJwtSignatureResponse.IsSuccessStatusCode)
                    {
                        var responseContent = await sdJwtSignatureResponse.Content.ReadAsStringAsync();
                        var signatureBytes = JObject.Parse(responseContent)["signature_bytes"]?.ToString();

                        presentation = $"{presentation[..presentation.LastIndexOf('.')]}.{signatureBytes}";
                    }

                    presentedCredential = sdJwt;
                    break;
                case MdocCredential mdocCredential:
                    format = FormatFun.CreateMdocFormat();

                    var toDisclose = claims.Select(claim =>
                        {
                            // TODO: This is needed because in mdoc the requested attributes look like this: $[Namespace][ElementId]. Refactor this more clean
                            var keys = claim.Split(new[] { "['", "']" }, StringSplitOptions.RemoveEmptyEntries);

                            var nameSpace = NameSpace.ValidNameSpace(keys[0]).UnwrapOrThrow();
                            var elementId = ElementIdentifier
                                .ValidElementIdentifier(keys[1])
                                .UnwrapOrThrow();

                            return (NameSpace: nameSpace, ElementId: elementId);
                        })
                        .GroupBy(nameSpaceAndElementId => nameSpaceAndElementId.NameSpace, tuple => tuple.ElementId)
                        .ToDictionary(group => group.Key, group => group.ToList());

                    var mdoc = mdocCredential.Mdoc.SelectivelyDisclose(toDisclose);

                    var handover = Handover.FromAuthorizationRequest(
                        authorizationRequest, 
                        Option<Origin>.None, 
                        authorizationRequest.ResponseMode == AuthorizationRequest.DirectPostJwt
                            ? await _verifierKeyService.GetPublicKey(authorizationRequest)
                            : Option<JsonWebKey>.None);
                    mdocNonce = handover.GetMdocNonce();
                    var sessionTranscript = handover.ToSessionTranscript();

                    var deviceNamespaces =
                        from keyAuths in mdoc.IssuerSigned.IssuerAuth.Payload.DeviceKeyInfo.KeyAuthorizations
                        select keyAuths.ToDeviceNameSpaces();

                    var deviceAuthentication = new DeviceAuthentication(
                        sessionTranscript, mdoc.DocType, deviceNamespaces);

                    var sigStructure = new SigStructure(deviceAuthentication.ToCbor(),
                        mdoc.IssuerSigned.IssuerAuth.ProtectedHeaders);

                    var sigStructureByteString = sigStructure.ToCbor();

                    var sigStructureHash = sha256.ComputeHash(sigStructureByteString.EncodeToBytes());

                    var mDocPostContent = new JObject
                    {
                        { "hash_bytes", Base64UrlEncoder.Encode(sigStructureHash) }
                    };

                    var mDocHttpContent =
                        new StringContent
                        (
                            mDocPostContent.ToString(),
                            Encoding.UTF8,
                            MediaTypeNames.Application.Json
                        );

                    var mDocSignatureResponse = await client.PostAsync(
                        session.AuthorizationData.IssuerMetadata.PresentationSigningEndpoint.UnwrapOrThrow(
                            new Exception()),
                        mDocHttpContent
                    );

                    if (mDocSignatureResponse.IsSuccessStatusCode)
                    {
                        var responseContent = await mDocSignatureResponse.Content.ReadAsStringAsync();
                        var signatureBytes = JObject.Parse(responseContent)["signature_bytes"]?.ToString();

                        var coseSignature = new CoseSignature(Base64UrlEncoder.DecodeBytes(signatureBytes));

                        var deviceSigned = new DeviceSignature(BuildProtectedHeaders(), coseSignature)
                            .ToDeviceSigned(deviceNamespaces);

                        presentation = new Document(new AuthenticatedMdoc(mdoc, deviceSigned)).BuildDeviceResponse()
                            .EncodeToBase64Url();
                    }

                    presentedCredential = mdocCredential;

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(credential.Credential));
            }

            presentations.Add((PresentationMap: new PresentationMap(credentialRequirementId, presentation, format), PresentedCredential: presentedCredential));
        }

        var authorizationResponse = await _oid4VpHaipClient.CreateAuthorizationResponseAsync(
            authorizationRequest,
            presentations.Select(tuple => tuple.PresentationMap).ToArray()
        );

        var content = authorizationRequest.ResponseMode switch
        {
            AuthorizationRequest.DirectPost => authorizationResponse.ToFormUrl(),
            AuthorizationRequest.DirectPostJwt => 
                (await _authorizationResponseEncryptionService.Encrypt(authorizationResponse, authorizationRequest, mdocNonce)).ToFormUrl(),
            _ => throw new ArgumentOutOfRangeException(nameof(authorizationRequest.ResponseMode))
        };

        var message = new HttpRequestMessage
        {
            RequestUri = new Uri(authorizationRequest.ResponseUri),
            Method = HttpMethod.Post,
            Content = content
        };

        // TODO: Introduce timeout
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Clear();
        var responseMessage = await httpClient.SendAsync(message);
        if (!responseMessage.IsSuccessStatusCode)
        {
            var str = await responseMessage.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Authorization Response failed with message {str}");
        }

        var presentedCredentials = presentations.Select(presentation =>
        {
            PresentedCredentialSet result;

            switch (presentation.PresentedCredential)
            {
                case SdJwtCredential sdJwtRecord:
                    var issuanceSdJwtDoc = sdJwtRecord.ToSdJwtDoc();
                    var sdJwtDoc = new SdJwtDoc(presentation.PresentationMap.Presentation);

                    var nonDisclosed =
                        from disclosure in issuanceSdJwtDoc.Disclosures
                        let base64Encoded = disclosure.Base64UrlEncoded
                        where sdJwtDoc.Disclosures.All(itm => itm.Base64UrlEncoded != base64Encoded)
                        select disclosure;

                    var presentedClaims =
                        from claim in sdJwtRecord.Claims
                        where !nonDisclosed.Any(nd => claim.Key.StartsWith(nd.Path ?? string.Empty))
                        select new
                        {
                            key = claim.Key,
                            value = new PresentedClaim { Value = claim.Value }
                        };

                    result = new PresentedCredentialSet
                    {
                        SdJwtCredentialType = Vct.ValidVct(sdJwtRecord.Vct).UnwrapOrThrow(),
                        CredentialSetId = CredentialSetId.ValidCredentialSetId(sdJwtRecord.CredentialSetId)
                            .UnwrapOrThrow(),
                        PresentedClaims = presentedClaims.ToDictionary(itm => itm.key, itm => itm.value)
                    };
                    break;
                case MdocCredential mdocCredential:
                    var claims = mdocCredential.Mdoc.IssuerSigned.IssuerNameSpaces.Value.SelectMany(pair => pair.Value);

                    result = new PresentedCredentialSet
                    {
                        MdocCredentialType = mdocCredential.Mdoc.DocType,
                        CredentialSetId = mdocCredential.GetCredentialSetId(),
                        PresentedClaims = claims.ToDictionary(
                            item => item.ElementId.ToString(),
                            item => new PresentedClaim { Value = item.Element.ToString() }
                        )
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(presentation.PresentedCredential));
            }

            return result;
        });

        var oidPresentationRecord = new CompletedPresentation(
            Guid.NewGuid().ToString(),
            authorizationRequest.ClientId!,
            presentedCredentials.ToList(),
            authorizationRequest.ClientMetadata,
            authorizationRequest.Requirements.Match(
                _ => Option<string>.None,
                presentationDefinition => presentationDefinition.Name),
            DateTime.UtcNow);

        await _presentationRepository.Add(oidPresentationRecord);
        
        var redirectUriJson = await responseMessage.Content.ReadAsStringAsync();

        try
        {
            Uri callbackUri = DeserializeObject<AuthorizationResponseCallback>(redirectUriJson);
            return callbackUri;
        }
        catch (Exception e)
        {
            _logger.LogWarning(
                e,
                "Could not parse Redirect URI received from: {ResponseUri}", authorizationRequest.ResponseUri);

            return null;
        }
    }

    public async Task<Validation<AuthorizationRequestCancellation, PresentationRequest>> ProcessAuthorizationRequestUri(
        AuthorizationRequestUri requestUri)
    {
        var authorizationRequestValidation = await _authorizationRequestService.GetAuthorizationRequest(requestUri);
        var result = authorizationRequestValidation.Map(async authRequest =>
        {
            var queryResult = await _candidateQueryService.Query(authRequest);
            var presentationCandidates = new PresentationRequest(authRequest, queryResult);
            
            var vpTxDataOption = presentationCandidates.AuthorizationRequest.TransactionData;

            var uc5TxDataOption = presentationCandidates
                .AuthorizationRequest
                .Requirements.Match(
                    _ => Option<IEnumerable<InputDescriptorTransactionData>>.None,
                    presentationDefinition => presentationDefinition.InputDescriptors.TraverseAny(descriptor =>
                    {
                        return
                            from list in descriptor.TransactionData
                            select new InputDescriptorTransactionData(descriptor.Id, list);
                    })
                );

            switch (vpTxDataOption.IsSome, uc5TxDataOption.IsSome)
            {
                case (true, false):
                case (true, true):
                {
                    var vpTxData = vpTxDataOption.UnwrapOrThrow();
                    return TransactionDataFun.ProcessVpTransactionData(presentationCandidates, vpTxData);
                }
                case (false, true):
                {
                    var uc5TxData = uc5TxDataOption.UnwrapOrThrow();
                    return TransactionDataFun.ProcessUc5TransactionData(presentationCandidates, uc5TxData);
                }
                default:
                    return presentationCandidates;
            }
        });

        return (await result.Traverse(candidates => candidates)).Flatten();
    }
}
