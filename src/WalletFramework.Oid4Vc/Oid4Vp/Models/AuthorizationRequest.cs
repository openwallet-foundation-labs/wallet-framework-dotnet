using System.Security.Cryptography.X509Certificates;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Errors;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;
using WalletFramework.Oid4Vc.Qes;
using static WalletFramework.Oid4Vc.Oid4Vp.Models.ClientIdScheme;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

/// <summary>
///     Represents the Request of a Verifier to a Holder within the OpenId4VP specification.
/// </summary>
public record AuthorizationRequest
{
    public const string DirectPost = "direct_post";
    public const string DirectPostJwt = "direct_post.jwt";

    private const string VpToken = "vp_token";

    public static readonly string[] SupportedClientIdSchemes =
        [RedirectUriScheme, VerifierAttestationScheme, X509SanDnsScheme];

    /// <summary>
    ///     Gets the client id scheme.
    /// </summary>
    [JsonProperty("client_id_scheme")]
    public ClientIdScheme ClientIdScheme { get; }

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
    ///     Gets the client id. The Identifier of the Verifier.
    /// </summary>
    [JsonProperty("client_id")]
    public string ClientId { get; }

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

    [JsonConstructor]
    private AuthorizationRequest(
        ClientIdScheme clientIdScheme,
        PresentationDefinition presentationDefinition,
        string clientId,
        string nonce,
        string responseUri,
        string responseMode,
        ClientMetadata? clientMetadata,
        string? clientMetadataUri,
        string? scope,
        string? state)
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

        ClientMetadata = clientMetadata;
        ClientMetadataUri = clientMetadataUri;
        Nonce = nonce;
        PresentationDefinition = presentationDefinition;
        ResponseUri = responseUri;
        ResponseMode = responseMode;
        Scope = scope;
        State = state;
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

    private static Validation<AuthorizationRequestCancellation, AuthorizationRequest> CreateAuthorizationRequest(
        JObject authRequestJObject)
    {
        if (IsHaipConform(authRequestJObject))
        {
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
            
            var responseUriOption = AuthorizationRequestExtensions.GetResponseUriMaybe(authRequestJObject);

            switch (transactionDataPropertyFoundValidation.IsSuccess, uc5TxDataFoundValidation.IsSuccess)
            {
                case (true, false):
                case (true, true):
                {
                    var txDataJArray = transactionDataPropertyFoundValidation.UnwrapOrThrow();
                    
                    var txDataValidation = 
                        from transactionDataArray in TransactionDataArray.FromJArray(txDataJArray)
                        from transactionDataEnum in transactionDataArray.Decode()
                        from authRequest in authRequestValidation
                        select authRequest with
                        {
                            TransactionData = transactionDataEnum.ToList()
                        };

                    return txDataValidation.ToLangExtValidation(responseUriOption);
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

                    var result =
                        from authRequest in authRequestValidation
                        from inputDescriptors in txDataValidation
                        select authRequest with
                        {
                            PresentationDefinition = authRequest.PresentationDefinition with
                            {
                                InputDescriptors = inputDescriptors.ToArray()
                            }
                        };

                    return result.ToLangExtValidation(responseUriOption);
                }
                default:
                    return authRequestValidation.ToLangExtValidation(responseUriOption);
            }
        }
        else
        {
            var responseUriOption = AuthorizationRequestExtensions.GetResponseUriMaybe(authRequestJObject.ToString());
            var error = new InvalidRequestError("The authorization request does not match the HAIP");
            return new AuthorizationRequestCancellation(responseUriOption, [error]);
        }
    }

    private static bool IsHaipConform(JObject authorizationRequestJson)
    {
        var responseType = authorizationRequestJson["response_type"]!.ToString();
        var responseUri = authorizationRequestJson["response_uri"]!.ToString();
        var responseMode = authorizationRequestJson["response_mode"]!.ToString();
        var redirectUri = authorizationRequestJson["redirect_uri"];
        var authorizationRequestClientId = authorizationRequestJson["client_id"]!.ToString();

        string clientId;
        string clientIdScheme;
        if (SupportedClientIdSchemes.Exists(supportedClientIdScheme =>
                authorizationRequestClientId.StartsWith($"{supportedClientIdScheme}:")))
        {
            clientIdScheme = authorizationRequestClientId.Split(':')[0];
            clientId = authorizationRequestClientId.Split(':')[1];
        }
        else
        {
            clientIdScheme = authorizationRequestJson["client_id_scheme"]!.ToString();
            clientId = authorizationRequestClientId;
        }

        return
            (responseType == VpToken
             && responseMode == DirectPost) || (responseMode == DirectPostJwt
                                                && !string.IsNullOrEmpty(responseUri)
                                                && redirectUri is null
                                                && (clientIdScheme is X509SanDnsScheme or VerifierAttestationScheme
                                                    || (clientIdScheme is RedirectUriScheme &&
                                                        clientId == responseUri)));
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

    internal static AuthorizationRequest WithX509(
        this AuthorizationRequest authorizationRequest,
        RequestObject requestObject)
    {
        var encodedCertificate = requestObject.GetLeafCertificate().GetEncoded();

        var certificates =
            requestObject
                .GetCertificates()
                .Select(x => x.GetEncoded())
                .Select(x => new X509Certificate2(x));

        var trustChain = new X509Chain();
        foreach (var element in certificates)
        {
            trustChain.ChainPolicy.ExtraStore.Add(element);
        }

        return authorizationRequest with
        {
            X509Certificate = new X509Certificate2(encodedCertificate),
            X509TrustChain = trustChain
        };
    }
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
