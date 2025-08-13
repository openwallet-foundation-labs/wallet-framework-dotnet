using LanguageExt;
using Newtonsoft.Json;
using OneOf;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Errors;
using WalletFramework.Oid4Vc.Oid4Vp.Jwk;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

/// <summary>
///    Represents the metadata of a client (verifier).
/// </summary>
public record ClientMetadata
{
    // Needed for Newtonsoft Json Serialization
    public ClientMetadata(
        string? authorizationEncryptedResponseEnc,
        string[]? encryptedResponseEncValuesSupported,
        string[] redirectUris,
        string? clientName,
        string? clientUri,
        string[]? contacts,
        string? logoUri,
        Option<JwkSet> jwks,
        string? jwksUri,
        string? policyUri,
        string? tosUri,
        Formats vpFormats,
        Formats vpFormatsSupported)
    {
        AuthorizationEncryptedResponseEnc = authorizationEncryptedResponseEnc;
        EncryptedResponseEncValuesSupported = encryptedResponseEncValuesSupported;
        RedirectUris = redirectUris;
        ClientName = clientName;
        ClientUri = clientUri;
        Contacts = contacts;
        LogoUri = logoUri;
        JwkSet = jwks;
        JwksUri = jwksUri;
        PolicyUri = policyUri;
        TosUri = tosUri;
        VpFormats = vpFormats;
        VpFormatsSupported = vpFormatsSupported;
    }

    /// <summary>
    ///     Defined the encoding that should be used when an encrypted Auth Response is requested by the verifier.
    ///     Replaced by encrypted_response_enc_values_supported but kept for now for backwards compatibility.
    /// </summary>
    [Obsolete("This property is obsolete.")]
    [JsonProperty("authorization_encrypted_response_enc")]
    public string? AuthorizationEncryptedResponseEnc { get; init; }

    /// <summary>
    ///     Defined the encoding that should be used when an encrypted Auth Response is requested by the verifier.
    /// </summary>
    [JsonProperty("encrypted_response_enc_values_supported")]
    public string[]? EncryptedResponseEncValuesSupported { get; init; }

    /// <summary>
    ///    The redirect URIs of the client (verifier).
    /// </summary>
    [JsonProperty("redirect_uris")]
    public string[] RedirectUris { get; init; }

    /// <summary>
    ///    The name of the client (verifier).
    /// </summary>
    [JsonProperty("client_name")]
    public string? ClientName { get; init; }

    /// <summary>
    ///     The URI of a web page providing information about the client (verifier).
    /// </summary>
    [JsonProperty("client_uri")]
    public string? ClientUri { get; init; }

    /// <summary>
    ///    The ways to contact people responsible for this client (verifier).
    /// </summary>
    [JsonProperty("contacts")]
    public string[]? Contacts { get; init; }

    /// <summary>
    ///     The URI of the logo of the client (verifier).
    /// </summary>
    [JsonProperty("logo_uri")]
    public string? LogoUri { get; init; }

    [JsonProperty("jwks")]
    [JsonConverter(typeof(ClientJwksConverter))]
    [JsonIgnore]
    public Option<JwkSet> JwkSet { get; init; }

    [JsonProperty("jwks_uri")] 
    public string? JwksUri { get; init; }

    /// <summary>
    ///     The URI to a human-readable privacy policy document for the client (verifier).
    /// </summary>
    [JsonProperty("policy_uri")]
    public string? PolicyUri { get; init; }

    /// <summary>
    ///     The URI to a human-readable terms of service document for the client (verifier).
    /// </summary>
    [JsonProperty("tos_uri")]
    public string? TosUri { get; init; }

    /// <summary>
    ///     The URI to a human-readable terms of service document for the client (verifier).
    ///     This is deprecated and replaced by vp_formats_supported but kept for now for backwards compatibility.
    /// </summary>
    [Obsolete("This property is obsolete.")]
    [JsonProperty("vp_formats")]
    public Formats? VpFormats { get; init; }

     /// <summary>
     ///     The URI to a human-readable terms of service document for the client (verifier).
     /// </summary>
     [JsonProperty("vp_formats_supported")]
     public Formats? VpFormatsSupported { get; init; }
}

public static class ClientMetadataExtensions
{
    public static Validation<AuthorizationRequestCancellation, Option<ClientMetadata>> VpFormatsSupportedValidation(this ClientMetadata clientMetadata, OneOf<DcqlQuery, PresentationDefinition> requirements, Option<Uri> responseUri)
    {
        return requirements.Match(
            dcql =>
            {
                var walletMetadata = WalletMetadata.CreateDefault();
                
                var (sdJwtRequested, mdocRequested) = 
                    (dcql.CredentialQueries.Any(query => query.Format == Constants.SdJwtDcFormat || query.Format == Constants.SdJwtVcFormat),
                        dcql.CredentialQueries.Any(query => query.Format == Constants.MdocFormat));
                
                return (sdJwtRequested, mdocRequested) switch
                {
                    (true, false) => IsSdJwtVpFormatSupported(clientMetadata, walletMetadata) 
                        ? clientMetadata.AsOption() 
                        : (Validation<AuthorizationRequestCancellation, Option<ClientMetadata>>) GetVpFormatsNotSupportedCancellation(responseUri),
                    (false, true) => IsMdocVpFormatSupported(clientMetadata, walletMetadata) 
                        ? clientMetadata.AsOption() 
                        : (Validation<AuthorizationRequestCancellation, Option<ClientMetadata>>) GetVpFormatsNotSupportedCancellation(responseUri),
                    (true, true) => IsSdJwtVpFormatSupported(clientMetadata, walletMetadata) && IsMdocVpFormatSupported(clientMetadata, walletMetadata) 
                        ? clientMetadata.AsOption() 
                        : (Validation<AuthorizationRequestCancellation, Option<ClientMetadata>>) GetVpFormatsNotSupportedCancellation(responseUri),
                    _ => clientMetadata.AsOption()
                };
            },
            _ => clientMetadata.AsOption());
    }
    
    private static bool IsMdocVpFormatSupported(ClientMetadata clientMetadata, WalletMetadata walletMetadata)
    {
        var rpSupportedVpFormats = clientMetadata.VpFormatsSupported ?? clientMetadata.VpFormats;
        var walletMetadataSupportedVpFormats = walletMetadata.VpFormatsSupported;

        if (rpSupportedVpFormats?.MDocFormat == null)
            return true;
        
        if (rpSupportedVpFormats.MDocFormat.IssuerAuthAlgValues != null && 
            !rpSupportedVpFormats.MDocFormat.IssuerAuthAlgValues.Any(clientAlg => walletMetadataSupportedVpFormats.MDocFormat!.IssuerAuthAlgValues!.Contains(clientAlg)))
            return false;
        
        if (rpSupportedVpFormats.MDocFormat.DeviceAuthAlgValues != null && 
            !rpSupportedVpFormats.MDocFormat.DeviceAuthAlgValues.Any(clientAlg => walletMetadataSupportedVpFormats.MDocFormat!.DeviceAuthAlgValues!.Contains(clientAlg)))
            return false;

        return true;
    }

    private static bool IsSdJwtVpFormatSupported(ClientMetadata clientMetadata, WalletMetadata walletMetadata)
    {
        var rpSupportedVpFormats = clientMetadata.VpFormatsSupported ?? clientMetadata.VpFormats;
        var walletMetadataSupportedVpFormats = walletMetadata.VpFormatsSupported;

        if (rpSupportedVpFormats?.SdJwtDcFormat != null)
        {
            if (rpSupportedVpFormats.SdJwtDcFormat.IssuerSignedJwtAlgValues != null && 
                !rpSupportedVpFormats.SdJwtDcFormat.IssuerSignedJwtAlgValues.Any(clientAlg => walletMetadataSupportedVpFormats.SdJwtDcFormat!.IssuerSignedJwtAlgValues!.Contains(clientAlg)))
                return false;
    
            if (rpSupportedVpFormats.SdJwtDcFormat.KeyBindingJwtAlgValues != null && 
                !rpSupportedVpFormats.SdJwtDcFormat.KeyBindingJwtAlgValues.Any(clientAlg => walletMetadataSupportedVpFormats.SdJwtDcFormat!.KeyBindingJwtAlgValues!.Contains(clientAlg)))
                return false;
        }

        //TODO: Remove SdJwtVcFormat in the future as it is deprecated (kept for now for backwards compatibility)
        if (rpSupportedVpFormats?.SdJwtVcFormat != null)
        {
            if (rpSupportedVpFormats.SdJwtVcFormat.IssuerSignedJwtAlgValues != null && 
                !rpSupportedVpFormats.SdJwtVcFormat.IssuerSignedJwtAlgValues.Any(clientAlg => walletMetadataSupportedVpFormats.SdJwtVcFormat!.IssuerSignedJwtAlgValues!.Contains(clientAlg)))
                return false;
    
            if (rpSupportedVpFormats.SdJwtVcFormat.KeyBindingJwtAlgValues != null && 
                !rpSupportedVpFormats.SdJwtVcFormat.KeyBindingJwtAlgValues.Any(clientAlg => walletMetadataSupportedVpFormats.SdJwtVcFormat!.KeyBindingJwtAlgValues!.Contains(clientAlg)))
                return false;
        }
        
        return true;
    }
    
    private static AuthorizationRequestCancellation GetVpFormatsNotSupportedCancellation(Option<Uri> responseUri)
    {
        var error = new VpFormatsNotSupportedError("The provided vp_formats_supported values are not supported");
        return new AuthorizationRequestCancellation(responseUri, [error]);
    }
}
