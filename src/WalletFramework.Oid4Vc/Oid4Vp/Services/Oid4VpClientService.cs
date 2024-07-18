using Hyperledger.Aries.Agents;
using Microsoft.Extensions.Logging;
using SD_JWT.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;
using static Newtonsoft.Json.JsonConvert;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

/// <inheritdoc />
public class Oid4VpClientService : IOid4VpClientService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Oid4VpClientService" /> class.
    /// </summary>
    /// <param name="agentProvider">The agent provider</param>
    /// <param name="httpClientFactory">The http client factory to create http clients.</param>
    /// <param name="sdJwtVcHolderService">The service responsible for SD-JWT related operations.</param>
    /// <param name="pexService">The Presentation Exchange service.</param>
    /// <param name="oid4VpHaipClient">The service responsible for OpenId4VP related operations.</param>
    /// <param name="logger">The ILogger.</param>
    /// <param name="oid4VpRecordService">The service responsible for OidPresentationRecord related operations.</param>
    public Oid4VpClientService(
        IAgentProvider agentProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<Oid4VpClientService> logger,
        IOid4VpHaipClient oid4VpHaipClient,
        IOid4VpRecordService oid4VpRecordService,
        IPexService pexService,
        ISdJwtVcHolderService sdJwtVcHolderService)
    {
        _agentProvider = agentProvider;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _oid4VpHaipClient = oid4VpHaipClient;
        _oid4VpRecordService = oid4VpRecordService;
        _pexService = pexService;
        _sdJwtVcHolderService = sdJwtVcHolderService;
    }

    private readonly IAgentProvider _agentProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Oid4VpClientService> _logger;
    private readonly IOid4VpHaipClient _oid4VpHaipClient;
    private readonly IOid4VpRecordService _oid4VpRecordService;
    private readonly IPexService _pexService;
    private readonly ISdJwtVcHolderService _sdJwtVcHolderService;

    /// <inheritdoc />
    public async Task<(AuthorizationRequest, IEnumerable<CredentialCandidates>)> ProcessAuthorizationRequestAsync(
        Uri authorizationRequestUri)
    {
        var haipAuthorizationRequestUri = HaipAuthorizationRequestUri.FromUri(authorizationRequestUri);

        var authorizationRequest = await _oid4VpHaipClient.ProcessAuthorizationRequestAsync(
            haipAuthorizationRequestUri
        );

        var context = await _agentProvider.GetContextAsync();
        var credentials = await _sdJwtVcHolderService.ListAsync(context);
            
        var credentialCandidates = await _pexService.FindCredentialCandidates(
            credentials.ToArray(),
            authorizationRequest.PresentationDefinition.InputDescriptors
        );

        return (authorizationRequest, credentialCandidates);
    }

    /// <inheritdoc />
    public async Task<Uri?> SendAuthorizationResponseAsync(
        AuthorizationRequest authorizationRequest,
        IEnumerable<SelectedCredential> selectedCredentials)
    {
        var createPresentationMaps =
            from credential in selectedCredentials
            from inputDescriptor in authorizationRequest.PresentationDefinition.InputDescriptors
            where credential.InputDescriptorId == inputDescriptor.Id
            let disclosedClaims = inputDescriptor
                .Constraints
                .Fields?
                .SelectMany(field => field.Path.Select(path => path.TrimStart('$', '.')))
            // TODO: MdocPresentation
            let createPresentation = _sdJwtVcHolderService.CreatePresentation(
                (SdJwtRecord)credential.Credential,
                disclosedClaims.ToArray(),
                authorizationRequest.ClientId,
                authorizationRequest.Nonce
            )
            select (inputDescriptor.Id, createPresentation);

        var presentationMaps = new List<(string, string)>();
        foreach (var (inputDescriptorId, createPresentation) in createPresentationMaps)
        {
            presentationMaps.Add((inputDescriptorId, await createPresentation));
        }

        var authorizationResponse = await _oid4VpHaipClient.CreateAuthorizationResponseAsync(
            authorizationRequest,
            presentationMaps.ToArray()
        );

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Clear();
            
        var responseMessage = await httpClient.SendAsync(
            new HttpRequestMessage
            {
                RequestUri = new Uri(authorizationRequest.ResponseUri),
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(
                    DeserializeObject<Dictionary<string, string>>(
                            SerializeObject(authorizationResponse)
                        )?
                        .ToList()
                    ?? throw new InvalidOperationException("Authorization Response could not be parsed")
                )
            }
        );

        if (!responseMessage.IsSuccessStatusCode)
            throw new InvalidOperationException("Authorization Response could not be sent");

        var presentedCredentials = presentationMaps
            .Join(
                selectedCredentials,
                presentation => presentation.Item1,
                selectedCredential => selectedCredential.InputDescriptorId,
                (presentation, selectedCredential) => new
                {
                    inputDescriptorId = presentation.Item1,
                    presentationFormat = presentation.Item2,
                    credential = selectedCredential.Credential
                }
            ).Select(presentationMapItem =>
            {
                var credentialRecord = (SdJwtRecord)presentationMapItem.credential;
                var issuanceSdJwtDoc = credentialRecord.ToSdJwtDoc();
                var presentationSdJwtDoc = new SdJwtDoc(presentationMapItem.presentationFormat);

                var nonDisclosedDisclosure =
                    from issuedDisclosures in issuanceSdJwtDoc.Disclosures
                    let base64Encoded = issuedDisclosures.Base64UrlEncoded
                    where presentationSdJwtDoc.Disclosures.All(itm => itm.Base64UrlEncoded != base64Encoded)
                    select issuedDisclosures;
                    
                var presentedClaims = 
                    from claim in credentialRecord.Claims
                    where !nonDisclosedDisclosure.Any(nd => claim.Key.StartsWith(nd.Path ?? string.Empty))
                    select new
                    {
                        key = claim.Key, 
                        value = new PresentedClaim { Value = claim.Value }  
                    };

                return new PresentedCredential
                {
                    CredentialId = credentialRecord.Id,
                    PresentedClaims = presentedClaims.ToDictionary(itm => itm.key, itm => itm.value)
                };
            });

        var context = await _agentProvider.GetContextAsync();

        await _oid4VpRecordService.StoreAsync(
            context,
            authorizationRequest.ClientId,
            authorizationRequest.ClientMetadata,
            authorizationRequest.PresentationDefinition.Name,
            presentedCredentials.ToArray());

        var redirectUriJson = await responseMessage.Content.ReadAsStringAsync();

        try
        {
            return DeserializeObject<AuthorizationResponseCallback>(redirectUriJson);

        }
        catch (Exception e)
        {
            _logger.LogWarning("Could not parse Redirect URI received from: {ResponseUri} due to exception: {Exception}", authorizationRequest.ResponseUri, e);
            return null;
        }
    }
}
    
internal static class SdJwtRecordExtensions
{
    internal static SdJwtDoc ToSdJwtDoc(this SdJwtRecord record)
    {
        return new SdJwtDoc(record.EncodedIssuerSignedJwt + "~" + string.Join("~", record.Disclosures) + "~");
    }
}
