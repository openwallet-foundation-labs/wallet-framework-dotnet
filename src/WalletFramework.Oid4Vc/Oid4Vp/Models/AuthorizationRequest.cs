using System.Security.Cryptography.X509Certificates;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Errors;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;
using WalletFramework.Oid4Vc.Qes.Authorization;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication;
using static WalletFramework.Oid4Vc.Oid4Vp.Models.ClientIdScheme;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

/// <summary>
///     Represents the Request of a Verifier to a Holder within the OpenId4VP specification.
/// </summary>
public record AuthorizationRequest
{
    public const string DirectPost = "direct_post";

    public const string DirectPostJwt = "direct_post.jwt";
    
    public const string DcApi = "dc_api";
    
    public const string DcApiJwt = "dc_api.jwt";

    private static readonly string[] SupportedClientIdSchemes =
        [RedirectUriScheme, VerifierAttestationScheme, X509SanDnsScheme];

    /// <summary>
    ///     Gets the client id scheme.
    /// </summary>
    [JsonProperty("client_id_scheme")]
    public ClientIdScheme? ClientIdScheme { get; }

    /// <summary>
    ///     Gets the client metadata. Contains the Verifier metadata.
    /// </summary>
    [JsonProperty("client_metadata")]
    public ClientMetadata? ClientMetadata { get; init; }

    [JsonIgnore]
    public Option<List<TransactionData>> TransactionData { get; private init; } = 
        Option<List<TransactionData>>.None;

    /// <summary>
    ///     Gets the presentation definition. Contains the claims that the Verifier wants to receive.
    /// </summary>
    [JsonProperty("presentation_definition")]
    public PresentationDefinition PresentationDefinition { get; init; }
    
    /// <summary>
    ///     Gets the DCQL query. Contains the claims that the Verifier wants to receive.
    /// </summary>
    [JsonProperty("dcql_query")]
    public DcqlQuery? DcqlQuery { get; }

    /// <summary>
    ///     Gets the client id. The Identifier of the Verifier.
    /// </summary>
    [JsonProperty("client_id")]
    public string? ClientId { get; }

    /// <summary>
    ///     Gets the nonce. Random string for session binding.
    /// </summary>
    [JsonProperty("nonce")]
    public string Nonce { get; }

    [JsonProperty("response_mode")] 
    public string ResponseMode { get; }

    /// <summary>
    ///     Gets the response mode. Determines where to send the Authorization Response to.
    /// </summary>
    [JsonProperty("response_uri")]
    public string ResponseUri { get; }

    /// <summary>
    ///     Gets the client metadata uri. Can be used to retrieve the verifier metadata.
    /// </summary>
    [JsonProperty("client_metadata_uri")]
    public string? ClientMetadataUri { get; }

    /// <summary>
    ///     The scope of the request.
    /// </summary>
    [JsonProperty("scope")]
    public string? Scope { get; }

    /// <summary>
    ///     Gets the state.
    /// </summary>
    [JsonProperty("state")]
    public string? State { get; }
    
    [JsonProperty("verifier_attestations")]
    [JsonConverter(typeof(VerifierAttestationsConverter))]
    public VerifierAttestation[]? VerifierAttestations { get; }

    /// <summary>
    ///     The X509 certificate of the verifier, this property is only set when ClientIDScheme is X509SanDNS.
    /// </summary>
    [JsonIgnore]
    public X509Certificate2? X509Certificate { get; init; }

    /// <summary>
    ///     The trust chain of the verifier, this property is only set when ClientIDScheme is X509SanDNS.
    /// </summary>
    [JsonIgnore]
    public X509Chain? X509TrustChain { get; init; }

    [JsonIgnore] 
    public RpAuthResult RpAuthResult { get; init; } = RpAuthResult.GetWithLevelUnknown();

    [JsonIgnore]
    public OneOf<DcqlQuery, PresentationDefinition> Requirements =>
        DcqlQuery is not null ? DcqlQuery : PresentationDefinition!;

    [JsonConstructor]
    private AuthorizationRequest(
        ClientIdScheme? clientIdScheme,
        PresentationDefinition presentationDefinition,
        DcqlQuery dcqlQuery,
        string? clientId,
        string nonce,
        string responseUri,
        string responseMode,
        ClientMetadata? clientMetadata,
        string? clientMetadataUri,
        string? scope,
        string? state,
        VerifierAttestation[] verifierAttestations)
    {
        if (clientId is not null)
        {
            if (SupportedClientIdSchemes.Exists(supportedClientIdScheme =>
                    clientId.StartsWith($"{supportedClientIdScheme}:")))
            {
                ClientIdScheme = clientId.Split(':')[0];
                ClientId = clientId.Split(':')[1];
            }
            else
            {
                ClientId = clientId;
                ClientIdScheme = clientIdScheme;
            }
        }

        ClientMetadata = clientMetadata;
        ClientMetadataUri = clientMetadataUri;
        Nonce = nonce;
        PresentationDefinition = presentationDefinition;
        DcqlQuery = dcqlQuery;
        ResponseUri = responseUri;
        ResponseMode = responseMode;
        Scope = scope;
        State = state;
        VerifierAttestations = verifierAttestations;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="AuthorizationRequest" /> class.
    /// </summary>
    /// <param name="authorizationRequestJson">The json representation of the authorization request.</param>
    /// <returns>A new instance of the <see cref="AuthorizationRequest" /> class.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the request does not match the HAIP.</exception>
    public static Validation<AuthorizationRequestCancellation, AuthorizationRequest> CreateAuthorizationRequest(
        string authorizationRequestJson)
    {
        JObject jObject;
        try
        {
            jObject = JObject.Parse(authorizationRequestJson);
        }
        catch (Exception e)
        {
            var responseUriOption = AuthorizationRequestExtensions.GetResponseUriMaybe(authorizationRequestJson);
            var error = new InvalidRequestError("The authorization request could not be parsed", e);
            return new AuthorizationRequestCancellation(responseUriOption, [error]);
        }

        try
        {
            return CreateAuthorizationRequest(jObject);
        }
        catch (Exception e)
        {
            var responseUriOption = AuthorizationRequestExtensions.GetResponseUriMaybe(authorizationRequestJson);
            var error = new InvalidRequestError("The authorization request could not be parsed", e);
            return new AuthorizationRequestCancellation(responseUriOption, [error]);
        }
    }

    public static Validation<AuthorizationRequestCancellation, AuthorizationRequest> CreateAuthorizationRequest(
        JObject authRequestJObject)
    {
        var responseUriOption = AuthorizationRequestExtensions.GetResponseUriMaybe(authRequestJObject);
            
        var authRequestValidation = 
            authRequestJObject.ToObject<AuthorizationRequest>()
            ?? new InvalidRequestError("Could not parse the Authorization Request")
                .ToInvalid<AuthorizationRequest>();
        
        var transactionDataPropertyFoundValidation =
            from jToken in authRequestJObject.GetByKey("transaction_data")
            from jArray in jToken.ToJArray()
            select jArray;

        var uc5TxDataFoundValidation =
            from presentationDefinitionToken in authRequestJObject.GetByKey("presentation_definition")
            from inputDescriptorsToken in presentationDefinitionToken.GetByKey("input_descriptors")
            from txDataArrays in inputDescriptorsToken.TraverseAny(descriptor =>
            {
                return
                    from txDataToken in descriptor.GetByKey("transaction_data")
                    from txDataArray in txDataToken.ToJArray()
                    select (descriptor, txDataArray);
            })
            select txDataArrays;

        switch (transactionDataPropertyFoundValidation.IsSuccess, uc5TxDataFoundValidation.IsSuccess)
        {
            case (true, false):
            case (true, true):
            {
                var txDataJArray = transactionDataPropertyFoundValidation.UnwrapOrThrow();

                var validation = authRequestValidation;
                authRequestValidation = 
                    from transactionDataArray in TransactionDataArray.FromJArray(txDataJArray)
                    from transactionDataEnum in transactionDataArray.Decode()
                    from authRequest in validation
                    select authRequest with
                    {
                        TransactionData = transactionDataEnum.ToList()
                    };
                break;
            }
            case (false, true):
            {
                var uc5TxDataJArray = uc5TxDataFoundValidation.UnwrapOrThrow();

                var txDataValidation = uc5TxDataJArray.TraverseAll(tuple =>
                {
                    var inputDescriptor = tuple.descriptor.ToObject<InputDescriptor>();
                    var txDataArray = tuple.txDataArray;

                    var inputDescriptorValidation =
                        from transactionDataArray in Uc5QesTransactionData.FromJArray(txDataArray)
                        let list = transactionDataArray.ToList()
                        select inputDescriptor with
                        {
                            TransactionData = list
                        };

                    return inputDescriptorValidation;
                });

                authRequestValidation =
                    from authRequest in authRequestValidation
                    from inputDescriptors in txDataValidation
                    select authRequest with
                    {
                        PresentationDefinition = authRequest.PresentationDefinition with
                        {
                            InputDescriptors = inputDescriptors.ToArray()
                        }
                    };

                break;
            }
        }
        
        return authRequestValidation.ToLangExtValidation(responseUriOption);
    }
}

internal static class AuthorizationRequestExtensions
{
    internal static Option<Uri> GetResponseUriMaybe(string authRequestJson)
    {
        try
        {
            var jObject = JObject.Parse(authRequestJson);
            return GetResponseUriMaybe(jObject);
        }
        catch (Exception)
        {
            return Option<Uri>.None;
        }
    }

    internal static Option<Uri> GetResponseUriMaybe(JObject authRequestJObject)
    {
        try
        {
            var responseUri = authRequestJObject["response_uri"]!.ToString();
            return new Uri(responseUri);
        }
        catch (Exception)
        {
            return Option<Uri>.None;
        }
    }

    internal static AuthorizationRequest WithClientMetadata(
        this AuthorizationRequest authorizationRequest,
        Option<ClientMetadata> clientMetadata)
        => authorizationRequest with { ClientMetadata = clientMetadata.ToNullable() };
}

public static class AuthorizationRequestFun
{
    public static Option<Uri> GetResponseUriMaybe(this AuthorizationRequest authRequest)
    {
        try
        {
            return new Uri(authRequest.ResponseUri);
        }
        catch (Exception)
        {
            return Option<Uri>.None;
        }
    }

    public static Validation<AuthorizationRequestCancellation, AuthorizationRequest> ToLangExtValidation(
        this Validation<AuthorizationRequest> authRequestValidation,
        Option<Uri> responseUriOption)
    {
        return authRequestValidation.Value.MapFail(error =>
        {
            var vpError = error as VpError ?? new InvalidRequestError("Could not parse the Authorization Request", error);
            return new AuthorizationRequestCancellation(responseUriOption, [vpError]);
        });
    }
}
