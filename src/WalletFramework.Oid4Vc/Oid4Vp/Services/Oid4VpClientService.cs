using System.Diagnostics;
using Hyperledger.Aries.Agents;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SD_JWT.Models;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Device.Abstractions;
using WalletFramework.MdocLib.Device.Response;
using WalletFramework.MdocLib.Security;
using WalletFramework.MdocVc;
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
    /// <param name="mdocAuthenticationService">The mdoc authentication service.</param>
    /// <param name="oid4VpHaipClient">The service responsible for OpenId4VP related operations.</param>
    /// <param name="logger">The ILogger.</param>
    /// <param name="oid4VpRecordService">The service responsible for OidPresentationRecord related operations.</param>
    public Oid4VpClientService(
        IAgentProvider agentProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<Oid4VpClientService> logger,
        IMdocAuthenticationService mdocAuthenticationService,
        IOid4VpHaipClient oid4VpHaipClient,
        IOid4VpRecordService oid4VpRecordService,
        IPexService pexService,
        ISdJwtVcHolderService sdJwtVcHolderService)
    {
        _agentProvider = agentProvider;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _mdocAuthenticationService = mdocAuthenticationService;
        _oid4VpHaipClient = oid4VpHaipClient;
        _oid4VpRecordService = oid4VpRecordService;
        _pexService = pexService;
        _sdJwtVcHolderService = sdJwtVcHolderService;
    }

    private readonly IAgentProvider _agentProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Oid4VpClientService> _logger;
    private readonly IMdocAuthenticationService _mdocAuthenticationService;
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

        // TODO: Delete this
        var x = JsonConvert.SerializeObject(authorizationRequest, Formatting.Indented);
        Debug.WriteLine($"AuthorizationRequest: {x} at {DateTime.Now:H:mm:ss:fff}");

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
        var credentials = selectedCredentials.ToList();
        
        var inputDescriptors = authorizationRequest
            .PresentationDefinition
            .InputDescriptors;
        
        var mdocNonce = new Option<Nonce>();

        var presentationMapTasks = credentials.Select(async credential =>
        {
            var inputDescriptor = inputDescriptors.Single(descriptor => 
                descriptor.Id == credential.InputDescriptorId);

            var claims =
                from field in inputDescriptor.Constraints.Fields
                from path in field.Path.Select(path => path.TrimStart('$', '.'))
                select path;

            
            string presentation;
            switch (credential.Credential)
            {
                case SdJwtRecord sdJwt:
                    presentation = await _sdJwtVcHolderService.CreatePresentation(
                        sdJwt,
                        claims.ToArray(),
                        authorizationRequest.ClientId,
                        authorizationRequest.Nonce);
                    break;
                case MdocRecord mdocRecord:
                    var mdoc = mdocRecord.Mdoc;
                    
                    // TODO: Fix this
                    // var nameSpace = NameSpace
                    //     .ValidNameSpace(inputDescriptor.Id)
                    //     .UnwrapOrThrow();
                    //
                    // var elements = claims.Select(claim =>
                    // {
                    //     var elementIdentifier = ElementIdentifier
                    //         .ValidElementIdentifier(claim)
                    //         .UnwrapOrThrow();
                    //
                    //     return elementIdentifier;
                    // });
                    //
                    // mdoc.SelectivelyDisclose(nameSpace, elements);
                    
                    var handover = authorizationRequest.ToVpHandover();
                    // TODO: Fix this
                    mdocNonce = handover.MdocGeneratedNonce;
                    var sessionTranscript = handover.ToSessionTranscript();
                    var authenticatedMdoc = await _mdocAuthenticationService.Authenticate(
                        mdoc, sessionTranscript, mdocRecord.KeyId);
                    
                    presentation = new Document(authenticatedMdoc).BuildDeviceResponse().EncodeBase64Url();
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(credential.Credential));
            }

            return (InputDescriptorId: inputDescriptor.Id, Presentation: presentation);
        });

        var presentationMaps = new List<(string InputDescriptorId, string Presentation)>();
        foreach (var task in presentationMapTasks)
        {
            var presentationMap = await task;
            presentationMaps.Add(presentationMap);
        }

        var authorizationResponse = await _oid4VpHaipClient.CreateAuthorizationResponseAsync(
            authorizationRequest,
            presentationMaps.ToArray()
        );

        var x = SerializeObject(authorizationResponse, Formatting.Indented);
        Debug.WriteLine($"AuthorizationResponse: {x} at {DateTime.Now:H:mm:ss:fff}");

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Clear();

        var json = SerializeObject(authorizationResponse);
        var nameValueCollection = DeserializeObject<Dictionary<string, string>>(json)!.ToList();
        var n = mdocNonce.UnwrapOrThrow(new InvalidOperationException());
        nameValueCollection.Add(KeyValuePair.Create<string, string>("apu", n.ToString()));
        var content = new FormUrlEncodedContent(nameValueCollection);

        var message = new HttpRequestMessage
        {
            RequestUri = new Uri(authorizationRequest.ResponseUri),
            Method = HttpMethod.Post,
            Content = content
        };
            
        // TODO: Introduce timeout
        var responseMessage = await httpClient.SendAsync(message);
        if (!responseMessage.IsSuccessStatusCode)
        {
            var str = await responseMessage.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Authorization Response could not be sent with message {str}");
        }

        Debug.WriteLine($"AuthResponse successfully sent at {DateTime.Now:H:mm:ss:fff}");

        // var tuples = presentationMaps.Join(
        //     credentials,
        //     presentation => presentation.InputDescriptorId,
        //     selectedCredential => selectedCredential.InputDescriptorId,
        //     (presentation, selectedCredential) => new
        //     {
        //         inputDescriptorId = presentation.InputDescriptorId,
        //         presentation = presentation.Presentation,
        //         credential = selectedCredential.Credential
        //     });
            
        // var presentedCredentials = tuples.Select(presentationMapItem =>
        // {
        //     var credentialRecord = (SdJwtRecord)presentationMapItem.credential;
        //     var issuanceSdJwtDoc = credentialRecord.ToSdJwtDoc();
        //     var presentationSdJwtDoc = new SdJwtDoc(presentationMapItem.presentation);
        //
        //     var nonDisclosed =
        //         from issuedDisclosures in issuanceSdJwtDoc.Disclosures
        //         let base64Encoded = issuedDisclosures.Base64UrlEncoded
        //         where presentationSdJwtDoc.Disclosures.All(itm => itm.Base64UrlEncoded != base64Encoded)
        //         select issuedDisclosures;
        //             
        //     var presentedClaims = 
        //         from claim in credentialRecord.Claims
        //         where !nonDisclosed.Any(nd => claim.Key.StartsWith(nd.Path ?? string.Empty))
        //         select new
        //         {
        //             key = claim.Key, 
        //             value = new PresentedClaim { Value = claim.Value }  
        //         };
        //
        //     return new PresentedCredential
        //     {
        //         CredentialId = credentialRecord.Id,
        //         PresentedClaims = presentedClaims.ToDictionary(itm => itm.key, itm => itm.value)
        //     };
        // });

        // var context = await _agentProvider.GetContextAsync();
        //
        // await _oid4VpRecordService.StoreAsync(
        //     context,
        //     authorizationRequest.ClientId,
        //     authorizationRequest.ClientMetadata,
        //     authorizationRequest.PresentationDefinition.Name,
        //     presentedCredentials.ToArray());

        var redirectUriJson = await responseMessage.Content.ReadAsStringAsync();

        try
        {
            return DeserializeObject<AuthorizationResponseCallback>(redirectUriJson);
        }
        catch (Exception e)
        {
            _logger.LogWarning(
                "Could not parse Redirect URI received from: {ResponseUri} due to exception: {Exception}",
                authorizationRequest.ResponseUri, 
                e);
            
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
