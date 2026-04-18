using LanguageExt;
using Microsoft.Extensions.Logging;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Functional;
using WalletFramework.MdocVc;
using WalletFramework.MdocVc.Persistence;
using WalletFramework.Oid4Vc.Errors;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Errors;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Persistence;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;
using WalletFramework.SdJwtLib.Models;
using WalletFramework.SdJwtVc;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.SdJwtVc.Persistence;
using WalletFramework.Storage;
using static Newtonsoft.Json.JsonConvert;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

/// <inheritdoc />
public class Oid4VpClientService : IOid4VpClientService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Oid4VpClientService" /> class.
    /// </summary>
    /// <param name="authorizationRequestService">The authorization request service.</param>
    /// <param name="authorizationResponseEncryptionService">The authorization response encryption service.</param>
    /// <param name="dcqlService">The DCQL service.</param>
    /// <param name="httpClientFactory">The http client factory to create http clients.</param>
    /// <param name="logger">The ILogger.</param>
    /// <param name="mdocRepository">The service responsible for mdoc storage operations.</param>
    /// <param name="sdJwtRepository">The service responsible for SD-JWT storage operations.</param>
    /// <param name="oid4VpHaipClient">The service responsible for OpenId4VP related operations.</param>
    /// <param name="presentationRepository">The service responsible for OidPresentationRecord related operations.</param>
    /// <param name="presentationService">The authorization response service.</param>
    public Oid4VpClientService(
        IAuthorizationRequestService authorizationRequestService,
        IAuthorizationResponseEncryptionService authorizationResponseEncryptionService,
        IDcqlService dcqlService,
        IHttpClientFactory httpClientFactory,
        ILogger<Oid4VpClientService> logger,
        IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId> mdocRepository,
        IDomainRepository<SdJwtCredential, SdJwtCredentialRecord, CredentialId> sdJwtRepository,
        IDomainRepository<CompletedPresentation, CompletedPresentationRecord, string> presentationRepository,
        IOid4VpHaipClient oid4VpHaipClient,
        IPresentationService presentationService)
    {
        _authorizationRequestService = authorizationRequestService;
        _authorizationResponseEncryptionService = authorizationResponseEncryptionService;
        _dcqlService = dcqlService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _mdocRepository = mdocRepository;
        _sdJwtRepository = sdJwtRepository;
        _oid4VpHaipClient = oid4VpHaipClient;
        _presentationRepository = presentationRepository;
        _presentationService = presentationService;
    }

    private readonly IAuthorizationRequestService _authorizationRequestService;
    private readonly IAuthorizationResponseEncryptionService _authorizationResponseEncryptionService;
    private readonly IDcqlService _dcqlService;
    private readonly IDomainRepository<CompletedPresentation, CompletedPresentationRecord, string> _presentationRepository;
    private readonly IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId> _mdocRepository;
    private readonly IDomainRepository<SdJwtCredential, SdJwtCredentialRecord, CredentialId> _sdJwtRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Oid4VpClientService> _logger;
    private readonly IOid4VpHaipClient _oid4VpHaipClient;
    private readonly IPresentationService _presentationService;

    public async Task<Option<Uri>> AbortAuthorizationRequest(AuthorizationRequestCancellation cancellation)
    {
        var callbackTaskOption = cancellation.ResponseUri.OnSome(async uri =>
        {
            var error = cancellation.Errors[0];

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
                var str = await responseMessage.Content.ReadAsStringAsync(cancellationSource.Token);
                throw new InvalidOperationException($"Authorization Error Response failed with message {str}");
            }

            var redirectUriJson = await responseMessage.Content.ReadAsStringAsync(cancellationSource.Token);
            var callback = DeserializeObject<AuthorizationResponseCallback>(redirectUriJson);
            return callback?.ToUri() ?? Option<Uri>.None;
        });

        var callbackUriOption = await callbackTaskOption.Traverse(uri => uri);
        return callbackUriOption.Flatten();
    }

    public async Task<Option<Uri>> AcceptAuthorizationRequest(
        AuthorizationRequest authorizationRequest,
        IEnumerable<SelectedCredential> selectedCredentials)
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
                (await _authorizationResponseEncryptionService.Encrypt(authorizationResponse, authorizationRequest,
                    mdocNonce)).ToFormUrl(),
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
                            if (mDocRecords.Count > 1)
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
            Option<string>.None,
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

    public async Task<Validation<AuthorizationRequestCancellation, PresentationRequest>> ProcessAuthorizationRequestUri(
        AuthorizationRequestUri requestUri)
    {
        var authorizationRequestValidation = await _authorizationRequestService.GetAuthorizationRequest(requestUri);
        var result = authorizationRequestValidation.Map(async authRequest =>
        {
            var queryResult = await _dcqlService.Query(authRequest.DcqlQuery);
            var presentationCandidates = new PresentationRequest(authRequest, queryResult);

            var vpTxDataOption = presentationCandidates.AuthorizationRequest.TransactionData;

            if (vpTxDataOption.IsSome)
            {
                var vpTxData = vpTxDataOption.UnwrapOrThrow();
                return TransactionDataFun.ProcessVpTransactionData(presentationCandidates, vpTxData);
            }

            return presentationCandidates;
        });

        return (await result.Traverse(candidates => candidates)).Flatten();
    }
}
