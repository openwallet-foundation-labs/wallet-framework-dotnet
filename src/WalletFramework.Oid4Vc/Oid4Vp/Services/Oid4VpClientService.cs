using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Extensions;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using SD_JWT.Models;
using WalletFramework.Oid4Vc.ClientAttestation;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Encoding;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib;
using WalletFramework.MdocLib.Device;
using WalletFramework.MdocLib.Device.Abstractions;
using WalletFramework.MdocLib.Device.Response;
using WalletFramework.MdocLib.Elements;
using WalletFramework.MdocLib.Security;
using WalletFramework.MdocLib.Security.Cose;
using WalletFramework.MdocVc;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption;
using WalletFramework.Oid4Vc.Oid4Vci.Extensions;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;
using static Newtonsoft.Json.JsonConvert;
using static WalletFramework.MdocLib.Security.Cose.ProtectedHeaders;

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
        IAuthFlowSessionStorage authFlowSessionStorage,
        ISdJwtVcHolderService sdJwtVcHolderService)
    {
        _agentProvider = agentProvider;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _mdocAuthenticationService = mdocAuthenticationService;
        _oid4VpHaipClient = oid4VpHaipClient;
        _oid4VpRecordService = oid4VpRecordService;
        _pexService = pexService;
        _authFlowSessionStorage = authFlowSessionStorage;
        _sdJwtVcHolderService = sdJwtVcHolderService;
    }

    private readonly IAgentProvider _agentProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Oid4VpClientService> _logger;
    private readonly IMdocAuthenticationService _mdocAuthenticationService;
    private readonly IOid4VpHaipClient _oid4VpHaipClient;
    private readonly IOid4VpRecordService _oid4VpRecordService;
    private readonly IPexService _pexService;
    private readonly IAuthFlowSessionStorage _authFlowSessionStorage;
    private readonly ISdJwtVcHolderService _sdJwtVcHolderService;

    /// <inheritdoc />
    public async Task<(AuthorizationRequest, IEnumerable<CredentialCandidates>)> ProcessAuthorizationRequestAsync(
        Uri authorizationRequestUri)
    {
        var haipAuthorizationRequestUri = HaipAuthorizationRequestUri.FromUri(authorizationRequestUri);

        var authorizationRequest = await _oid4VpHaipClient.ProcessAuthorizationRequestAsync(
            haipAuthorizationRequestUri
        );

        var credentialCandidates = await _pexService.FindCredentialCandidates(
            authorizationRequest.PresentationDefinition.InputDescriptors);

        return (authorizationRequest, credentialCandidates);
    }

    /// <inheritdoc />
    public async Task<Uri?> SendAuthorizationResponseAsync(
        AuthorizationRequest authorizationRequest,
        IEnumerable<SelectedCredential> selectedCredentials,
        CombinedWalletAttestation? clientAttestation = null)
    {
        var credentials = selectedCredentials.ToList();
        
        var inputDescriptors = authorizationRequest
            .PresentationDefinition
            .InputDescriptors;
        
        // TODO: This is only a hack until the encryption response is implemented
        var mdocNonce = Option<Nonce>.None;

        var presentationMapTasks = credentials.Select(async credential =>
        {
            var inputDescriptor = inputDescriptors.Single(descriptor => 
                descriptor.Id == credential.InputDescriptorId);

            var claims =
                from field in inputDescriptor.Constraints.Fields
                from path in field.Path.Select(path => path.TrimStart('$', '.'))
                select path;

            Format format;
            ICredential presentedCredential;

            string presentation;
            switch (credential.Credential)
            {
                case SdJwtRecord sdJwt:
                    format = FormatFun.CreateSdJwtFormat();
                    
                    presentation = await _sdJwtVcHolderService.CreatePresentation(
                        sdJwt,
                        claims.ToArray(),
                        authorizationRequest.ClientId,
                        authorizationRequest.Nonce);

                    presentedCredential = sdJwt;
                    break;
                case MdocRecord mdocRecord:
                    format = FormatFun.CreateMdocFormat();
                    
                    var toDisclose = claims.Select(claim =>
                    {
                        // TODO: This is needed because in mdoc the requested attributes look like this: $[Namespace][ElementId]. Refactor this more clean
                        var keys = claim.Split(new[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                        
                        var nameSpace = NameSpace.ValidNameSpace(keys[0]).UnwrapOrThrow();
                        var elementId = ElementIdentifier
                            .ValidElementIdentifier(keys[1])
                            .UnwrapOrThrow();

                        return (NameSpace: nameSpace, ElementId: elementId);
                    })
                    .GroupBy(nameSpaceAndElementId => nameSpaceAndElementId.NameSpace, tuple => tuple.ElementId)
                    .ToDictionary(group => group.Key, group => group.ToList());
                    
                    var mdoc = mdocRecord.Mdoc.SelectivelyDisclose(toDisclose);

                    var handover = authorizationRequest.ToVpHandover();
                    mdocNonce = handover.MdocGeneratedNonce;
                    var sessionTranscript = handover.ToSessionTranscript();
                    var authenticatedMdoc = await _mdocAuthenticationService.Authenticate(
                        mdoc, sessionTranscript, mdocRecord.KeyId);
                    
                    presentation = new Document(authenticatedMdoc).BuildDeviceResponse().EncodeToBase64Url();

                    presentedCredential = mdocRecord;
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(credential.Credential));
            }

            return (InputDescriptorId: inputDescriptor.Id, Presentation: presentation, Format: format, PresentedCredential: presentedCredential);
        });

        var presentationMaps = new List<(string InputDescriptorId, string Presentation, Format Format, ICredential PresentedCredential)>();
        foreach (var task in presentationMapTasks)
        {
            var presentationMap = await task;
            presentationMaps.Add(presentationMap);
        }

        var authorizationResponse = await _oid4VpHaipClient.CreateAuthorizationResponseAsync(
            authorizationRequest,
            presentationMaps.Select(tuple => (tuple.InputDescriptorId, tuple.Presentation, tuple.Format)).ToArray()
        );

        var content = authorizationRequest.ResponseMode switch
        {
            AuthorizationRequest.DirectPost => authorizationResponse.ToFormUrl(),
            AuthorizationRequest.DirectPostJwt => authorizationResponse.Encrypt(authorizationRequest, mdocNonce).ToFormUrl(),
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
        
        var presentedCredentials = presentationMaps.Select(presentationMap =>
        {
            PresentedCredential result;
            
            switch (presentationMap.PresentedCredential)
            {
                case SdJwtRecord sdJwtRecord:
                    var issuanceSdJwtDoc = sdJwtRecord.ToSdJwtDoc();
                    var presentation = new SdJwtDoc(presentationMap.Presentation);
                    
                    var nonDisclosed =
                        from disclosure in issuanceSdJwtDoc.Disclosures
                        let base64Encoded = disclosure.Base64UrlEncoded
                        where presentation.Disclosures.All(itm => itm.Base64UrlEncoded != base64Encoded)
                        select disclosure;
                    
                    var presentedClaims = 
                        from claim in sdJwtRecord.Claims
                        where !nonDisclosed.Any(nd => claim.Key.StartsWith(nd.Path ?? string.Empty))
                        select new
                        {
                            key = claim.Key, 
                            value = new PresentedClaim { Value = claim.Value }  
                        };
        
                    result = new PresentedCredential
                    {
                        CredentialId = sdJwtRecord.Id,
                        PresentedClaims = presentedClaims.ToDictionary(itm => itm.key, itm => itm.value)
                    };
                    break;
                case MdocRecord mdocRecord:
                    var claims = mdocRecord.Mdoc.IssuerSigned.IssuerNameSpaces.Value.SelectMany(pair => pair.Value);

                    result = new PresentedCredential
                    {
                        CredentialId = mdocRecord.CredentialId,
                        PresentedClaims = claims.ToDictionary(
                            item => item.ElementId.ToString(),
                            item => new PresentedClaim { Value = item.Element.ToString() }
                        )
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(presentationMap.PresentedCredential));
            }

            return result;
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
            _logger.LogWarning(
                e, 
                "Could not parse Redirect URI received from: {ResponseUri}", authorizationRequest.ResponseUri);
            
            return null;
        }
    }
    
    //TODO: Refactor this C'' method into current flows (too much duplicate code)
    public async Task<Uri?> SendAuthorizationResponseAsync(AuthorizationRequest authorizationRequest, IEnumerable<SelectedCredential> selectedCredentials,
        IssuanceSession issuanceSession, CombinedWalletAttestation? clientAttestation = null)
    {
        var credentials = selectedCredentials.ToList();
        
        var inputDescriptors = authorizationRequest
            .PresentationDefinition
            .InputDescriptors;
        
        // TODO: This is only a hack until the encryption response is implemented
        var mdocNonce = Option<Nonce>.None;

        var presentationMapTasks = credentials.Select(async credential =>
        {
            var inputDescriptor = inputDescriptors.Single(descriptor => 
                descriptor.Id == credential.InputDescriptorId);

            var claims =
                from field in inputDescriptor.Constraints.Fields
                from path in field.Path.Select(path => path.TrimStart('$', '.'))
                select path;

            Format format;
            ICredential presentedCredential;

            var context = await _agentProvider.GetContextAsync();
            var session = await _authFlowSessionStorage.GetAsync(context, issuanceSession.AuthFlowSessionState);
            
            var presentation = string.Empty;
            switch (credential.Credential)
            {
                case SdJwtRecord sdJwt:
                    format = FormatFun.CreateSdJwtFormat();
                    
                    presentation = await _sdJwtVcHolderService.CreatePresentation(
                        sdJwt,
                        claims.ToArray(),
                        authorizationRequest.ClientId,
                        authorizationRequest.Nonce);

                    var kbJwt = presentation[presentation.LastIndexOf('~')..][1..];
                    var kbJwtWithoutSignature = kbJwt[..kbJwt.LastIndexOf('.')];

                    var sha256 = SHA256.Create();
                    var content = new JObject();
                    // var test = "eyJhbGciOiJFUzI1NiJ9.eyJpc3MiOiJqb2UiLA0KICJleHAiOjEzMDA4MTkzODAsDQogImh0dHA6Ly9leGFtcGxlLmNvbS9pc19yb290Ijp0cnVlfQ";
                    // var haher = sha256.ComputeHash(test.GetUTF8Bytes());
                    // var base65 = Base64UrlEncoder.Encode(haher);
                    var hash = sha256.ComputeHash(kbJwtWithoutSignature.GetUTF8Bytes());
                    content.Add("hash_bytes", Base64UrlEncoder.Encode(hash));

                    var contentAsString = content.ToString();
                    var httpContent =
                        new StringContent
                        (
                            contentAsString,
                            Encoding.UTF8,
                            MediaTypeNames.Application.Json
                        );
        
                    var client = _httpClientFactory.CreateClient();
                    client.DefaultRequestHeaders.Remove("Authorization");
                    client.DefaultRequestHeaders.Add(
                        "Authorization",
                        $"DPoP {session.AuthorizationData.AccessToken}");
                    
                    var response = await client.PostAsync(new Uri("https://demo.pid-issuer.bundesdruckerei.de/c2/presentation-signing"), httpContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var signatureBytes = JObject.Parse(responseContent)["signature_bytes"]?.ToString();

                        var pre = presentation[..presentation.LastIndexOf('~')];
                        presentation = $"{pre}~{kbJwtWithoutSignature}.{signatureBytes}";
                    }
                    
                    presentedCredential = sdJwt;
                    break;
                case MdocRecord mdocRecord:
                    format = FormatFun.CreateMdocFormat();
                    
                    var toDisclose = claims.Select(claim =>
                    {
                        var keys = claim.Split(new[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                        
                        var nameSpace = NameSpace.ValidNameSpace(keys[0]).UnwrapOrThrow();
                        var elementId = ElementIdentifier
                            .ValidElementIdentifier(keys[1])
                            .UnwrapOrThrow();

                        return (NameSpace: nameSpace, ElementId: elementId);
                    })
                    .GroupBy(nameSpaceAndElementId => nameSpaceAndElementId.NameSpace, tuple => tuple.ElementId)
                    .ToDictionary(group => group.Key, group => group.ToList());
                    
                    var mdoc = mdocRecord.Mdoc.SelectivelyDisclose(toDisclose);

                    var handover = authorizationRequest.ToVpHandover();
                    mdocNonce = handover.MdocGeneratedNonce;
                    var sessionTranscript = handover.ToSessionTranscript();
                    
                    
                    
                    var deviceNamespaces =
                        from keyAuths in mdoc.IssuerSigned.IssuerAuth.Payload.DeviceKeyInfo.KeyAuthorizations
                        select keyAuths.ToDeviceNameSpaces();

                    var deviceAuthentication = new DeviceAuthentication(
                        sessionTranscript, mdoc.DocType, deviceNamespaces);
                    
                    var sha2562 = SHA256.Create();
                    var hashdhef = sha2562.ComputeHash(deviceAuthentication.ToCbor().ToJSONBytes());
                    
                    var mDocPostContent = new JObject();
                    mDocPostContent.Add("hash_bytes", Base64UrlEncoder.Encode(hashdhef));

                    var contentAsString2 = mDocPostContent.ToString();
                    var httpContent2 =
                        new StringContent
                        (
                            contentAsString2,
                            Encoding.UTF8,
                            MediaTypeNames.Application.Json
                        );
        
                    var client2 = _httpClientFactory.CreateClient();
                    client2.DefaultRequestHeaders.Remove("Authorization");
                    client2.DefaultRequestHeaders.Add(
                        "Authorization",
                        $"DPoP {session.AuthorizationData.AccessToken}");
                    
                    var response2 = await client2.PostAsync(new Uri("https://demo.pid-issuer.bundesdruckerei.de/c2/presentation-signing"), httpContent2);

                    if (response2.IsSuccessStatusCode)
                    {
                        var responseContent = await response2.Content.ReadAsStringAsync();
                        var signatureBytes = JObject.Parse(responseContent)["signature_bytes"]?.ToString();

                        var coseSignature = new CoseSignature(signatureBytes.GetUTF8Bytes());
                        
                        var deviceSigned = new DeviceSignature(BuildProtectedHeaders(), coseSignature)
                            .ToDeviceSigned(deviceNamespaces);
                        
                        presentation = new Document(new AuthenticatedMdoc(mdoc, deviceSigned)).BuildDeviceResponse().EncodeToBase64Url();
                    }
                    
                    
                    // var authenticatedMdoc = await _mdocAuthenticationService.Authenticate(
                    //     mdoc, sessionTranscript, mdocRecord.KeyId);
                    
                    

                    presentedCredential = mdocRecord;
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(credential.Credential));
            }

            return (InputDescriptorId: inputDescriptor.Id, Presentation: presentation, Format: format, PresentedCredential: presentedCredential);
        });

        var presentationMaps = new List<(string InputDescriptorId, string Presentation, Format Format, ICredential PresentedCredential)>();
        foreach (var task in presentationMapTasks)
        {
            var presentationMap = await task;
            presentationMaps.Add(presentationMap);
        }

        var authorizationResponse = await _oid4VpHaipClient.CreateAuthorizationResponseAsync(
            authorizationRequest,
            presentationMaps.Select(tuple => (tuple.InputDescriptorId, tuple.Presentation, tuple.Format)).ToArray()
        );

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Clear();
        if (clientAttestation is not null)
            httpClient.AddClientAttestationPopHeader(clientAttestation);

        var json = SerializeObject(authorizationResponse);
        var nameValueCollection = DeserializeObject<Dictionary<string, string>>(json)!.ToList();
        
        // TODO: This is only a hack until the encryption response is implemented
        mdocNonce.IfSome(nonce =>
        {
            var pair = new KeyValuePair<string, string>("apu", nonce.AsBase64Url.ToString());
            nameValueCollection.Add(pair);
        });
        
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
        
        var presentedCredentials = presentationMaps.Select(presentationMap =>
        {
            PresentedCredential result;
            
            switch (presentationMap.PresentedCredential)
            {
                case SdJwtRecord sdJwtRecord:
                    var issuanceSdJwtDoc = sdJwtRecord.ToSdJwtDoc();
                    var presentation = new SdJwtDoc(presentationMap.Presentation);
                    
                    var nonDisclosed =
                        from disclosure in issuanceSdJwtDoc.Disclosures
                        let base64Encoded = disclosure.Base64UrlEncoded
                        where presentation.Disclosures.All(itm => itm.Base64UrlEncoded != base64Encoded)
                        select disclosure;
                    
                    var presentedClaims = 
                        from claim in sdJwtRecord.Claims
                        where !nonDisclosed.Any(nd => claim.Key.StartsWith(nd.Path ?? string.Empty))
                        select new
                        {
                            key = claim.Key, 
                            value = new PresentedClaim { Value = claim.Value }  
                        };
        
                    result = new PresentedCredential
                    {
                        CredentialId = sdJwtRecord.Id,
                        PresentedClaims = presentedClaims.ToDictionary(itm => itm.key, itm => itm.value)
                    };
                    break;
                case MdocRecord mdocRecord:
                    var claims = mdocRecord.Mdoc.IssuerSigned.IssuerNameSpaces.Value.SelectMany(pair => pair.Value);

                    result = new PresentedCredential
                    {
                        CredentialId = mdocRecord.CredentialId,
                        PresentedClaims = claims.ToDictionary(
                            item => item.ElementId.ToString(),
                            item => new PresentedClaim { Value = item.Element.ToString() }
                        )
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(presentationMap.PresentedCredential));
            }

            return result;
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
            _logger.LogWarning(
                e, 
                "Could not parse Redirect URI received from: {ResponseUri}", authorizationRequest.ResponseUri);
            
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
