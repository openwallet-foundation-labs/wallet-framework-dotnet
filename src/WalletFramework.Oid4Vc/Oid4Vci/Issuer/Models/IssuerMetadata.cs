using System.Globalization;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.SdJwt;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Errors;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Uri;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;
using static WalletFramework.Core.Functional.ValidationFun;
using static WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models.AuthorizationServerId;
using static WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models.CredentialIssuerId;
using static WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models.CredentialConfigurationId;
using static WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer.IssuerDisplay;
using static WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models.IssuerMetadataJsonExtensions;

namespace WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;

/// <summary>
///     Represents the metadata of an OpenID4VCI Credential Issuer.
/// </summary>
public record IssuerMetadata
{
    /// <summary>
    ///     Gets a dictionary which maps a CredentialConfigurationId to its credential metadata.
    /// </summary>
    public Dictionary<CredentialConfigurationId, SupportedCredentialConfiguration> CredentialConfigurationsSupported { get; }
    
    /// <summary>
    ///     Gets a list of display properties of a Credential Issuer for different languages.
    /// </summary>
    public Option<List<IssuerDisplay>> Display { get; }

    /// <summary>
    ///     Gets the URL of the Credential Issuer's Credential Endpoint.
    /// </summary>
    public Uri CredentialEndpoint { get; }
    
    /// <summary>
    ///     Gets the URL of the Presentation Signing Endpoint Endpoint.
    /// </summary>
    public Option<Uri> PresentationSigningEndpoint { get; }

    /// <summary>
    ///     Gets the identifier of the Credential Issuer.
    /// </summary>
    public CredentialIssuerId CredentialIssuer { get; }

    /// <summary>
    ///     Gets the identifier of the OAuth 2.0 Authorization Server that the Credential Issuer relies on for
    ///     authorization. If this property is omitted, it is assumed that the entity providing the Credential Issuer
    ///     is also acting as the Authorization Server. In such cases, the Credential Issuer's
    ///     identifier is used as the OAuth 2.0 Issuer value to obtain the Authorization Server
    ///     metadata.
    /// </summary>
    public Option<IEnumerable<AuthorizationServerId>> AuthorizationServers { get; }
    
    private IssuerMetadata(
        Dictionary<CredentialConfigurationId, SupportedCredentialConfiguration> credentialConfigurationsSupported,
        Option<List<IssuerDisplay>> display,
        Uri credentialEndpoint,
        Option<Uri> presentationSigningEndpoint,
        CredentialIssuerId credentialIssuer,
        Option<IEnumerable<AuthorizationServerId>> authorizationServers)
    {
        CredentialConfigurationsSupported = credentialConfigurationsSupported;
        Display = display;
        CredentialEndpoint = credentialEndpoint;
        PresentationSigningEndpoint = presentationSigningEndpoint;
        CredentialIssuer = credentialIssuer;
        AuthorizationServers = authorizationServers;
    }
    
    private static IssuerMetadata Create(
        Dictionary<CredentialConfigurationId, SupportedCredentialConfiguration> credentialConfigurationsSupported,
        Option<List<IssuerDisplay>> display,
        Uri credentialEndpoint,
        Option<Uri> presentationSigningEndpoint,
        CredentialIssuerId credentialIssuer,
        Option<IEnumerable<AuthorizationServerId>> authorizationServers) => new(
        credentialConfigurationsSupported,
        display,
        credentialEndpoint,
        presentationSigningEndpoint,
        credentialIssuer,
        authorizationServers);

    public static Validation<IssuerMetadata> ValidIssuerMetadata(JObject json)
    {
        var credentialConfigurations =
            from jToken in json.GetByKey(CredentialConfigsSupportedJsonKey)
            from jObj in jToken.ToJObject()
            from dict in jObj.ToValidDictionaryAny(ValidCredentialConfigurationId, token =>
            {
                var sdJwt = SdJwtConfiguration.ValidSdJwtCredentialConfiguration(token);

                if (sdJwt.Value.IsSuccess)
                {
                    return sdJwt.OnSuccess(configuration =>
                    {
                        SupportedCredentialConfiguration oneOf = configuration;
                        return oneOf;
                    });
                }
                else
                {
                    var mdoc = token
                        .ToJObject()
                        .OnSuccess(MdocConfiguration.ValidMdocConfiguration);
                    
                    return mdoc.OnSuccess(configuration =>
                    {
                        SupportedCredentialConfiguration oneOf = configuration;
                        return oneOf;
                    });
                }
            })
            select dict;

        var display =
            from jToken in json.GetByKey(DisplayJsonKey).ToOption()
            from jArray in jToken.ToJArray().ToOption()
            from result in jArray.TraverseAny(OptionalIssuerDisplay)
            select result.ToList();

        var credentialEndpoint =
            from jToken in json.GetByKey(CredentialEndpointJsonKey)
            from endpoint in jToken.ToJValue().OnSuccess(value =>
            {
                try
                {
                    var str = value.ToString(CultureInfo.InvariantCulture);
                    return new Uri(str);
                }
                catch (Exception e)
                {
                    return new CredentialEndpointError(e).ToInvalid<Uri>();
                }
            })
            select endpoint;
        
        var presentationSigningEndpoint =
            from jToken in json.GetByKey(PresentationSigningEndpointJsonKey).ToOption()
            select new Uri(jToken.ToString());

        var credentialIssuerId = json
            .GetByKey(CredentialIssuerJsonKey)
            .OnSuccess(ValidCredentialIssuerId);

        var authServers = 
            from jToken in json.GetByKey(AuthorizationServersJsonKey).ToOption()
            from jArray in jToken.ToJArray().ToOption()
            from serverIds in jArray
                .TraverseAny(token => ValidAuthorizationServerId(token).ToOption())
            select serverIds;

        return Valid(Create)
            .Apply(credentialConfigurations)
            .Apply(display)
            .Apply(credentialEndpoint)
            .Apply(presentationSigningEndpoint)
            .Apply(credentialIssuerId)
            .Apply(authServers);
    }
}

public static class IssuerMetadataJsonExtensions
{
    public const string CredentialConfigsSupportedJsonKey = "credential_configurations_supported";
    public const string DisplayJsonKey = "display";
    public const string CredentialEndpointJsonKey = "credential_endpoint";
    public const string PresentationSigningEndpointJsonKey = "presentation_signing_endpoint";
    public const string CredentialIssuerJsonKey = "credential_issuer";
    public const string AuthorizationServersJsonKey = "authorization_servers";
    
    public static JObject EncodeToJson(this IssuerMetadata issuerMetadata)
    {
        var result = new JObject();
        
        var configsJson = new JObject();
        foreach (var (key, config) in issuerMetadata.CredentialConfigurationsSupported)
        {
            var configJson = config.Match(
                sdJwt => sdJwt.EncodeToJson(),
                mdoc => mdoc.EncodeToJson()
            );

            configsJson.Add(key.ToString(), configJson);
        }
        result.Add(CredentialConfigsSupportedJsonKey, configsJson);

        issuerMetadata.Display.IfSome(displays =>
        {
            var displaysJson = new JArray();
            foreach (var display in displays)
            {
                displaysJson.Add(display.EncodeToJson());
            }
            result.Add(DisplayJsonKey, displaysJson);
        });
        
        result.Add(CredentialEndpointJsonKey, issuerMetadata.CredentialEndpoint.ToStringWithoutTrail());
        issuerMetadata.PresentationSigningEndpoint.IfSome(endpoint =>
        {
            result.Add(PresentationSigningEndpointJsonKey, endpoint.ToStringWithoutTrail());
        });
        result.Add(CredentialIssuerJsonKey, issuerMetadata.CredentialIssuer.ToString());
        
        var authServersJson = new JArray();
        issuerMetadata.AuthorizationServers.IfSome(servers =>
        {
            foreach (var server in servers)
            {
                authServersJson.Add(server.ToString());
            }
        });
        result.Add(AuthorizationServersJsonKey, authServersJson);
        
        return result;
    }
}
