using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using Hyperledger.Aries.Extensions;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.SdJwt;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Errors;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;
using static WalletFramework.Core.Functional.ValidationFun;
using static WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models.AuthorizationServerId;
using static WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models.CredentialIssuerId;
using static WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models.CredentialConfigurationId;
using static WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer.IssuerDisplay;

namespace WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;

/// <summary>
///     Represents the metadata of an OpenID4VCI Credential Issuer.
/// </summary>
[JsonConverter(typeof(IssuerMetadataJsonConverter))]
public record IssuerMetadata
{
    // Do not change the order of this property (must be last) otherwise the JSON serialization will not
    // work properly...
    /// <summary>
    ///     Gets a dictionary which maps a CredentialConfigurationId to its credential metadata.
    /// </summary>
    [JsonProperty(CredentialConfigsSupportedJsonKey)]
    [JsonConverter(typeof(DictJsonConverter<CredentialConfigurationId, SupportedCredentialConfiguration>))]
    public Dictionary<CredentialConfigurationId, SupportedCredentialConfiguration> CredentialConfigurationsSupported { get; }
    
    /// <summary>
    ///     Gets a list of display properties of a Credential Issuer for different languages.
    /// </summary>
    [JsonProperty(DisplayJsonKey)]
    [JsonConverter(typeof(OptionJsonConverter<List<IssuerDisplay>>))]
    public Option<List<IssuerDisplay>> Display { get; }

    /// <summary>
    ///     Gets the URL of the Credential Issuer's Credential Endpoint.
    /// </summary>
    [JsonProperty(CredentialEndpointJsonKey)]
    public Uri CredentialEndpoint { get; }

    /// <summary>
    ///     Gets the identifier of the Credential Issuer.
    /// </summary>
    [JsonProperty(CredentialIssuerJsonKey)]
    public CredentialIssuerId CredentialIssuer { get; }

    /// <summary>
    ///     Gets the identifier of the OAuth 2.0 Authorization Server that the Credential Issuer relies on for
    ///     authorization. If this property is omitted, it is assumed that the entity providing the Credential Issuer
    ///     is also acting as the Authorization Server. In such cases, the Credential Issuer's
    ///     identifier is used as the OAuth 2.0 Issuer value to obtain the Authorization Server
    ///     metadata.
    /// </summary>
    [JsonProperty(AuthorizationServersJsonKey)]
    [JsonConverter(typeof(OptionJsonConverter<IEnumerable<AuthorizationServerId>>))]
    public Option<IEnumerable<AuthorizationServerId>> AuthorizationServers { get; }
        
    private IssuerMetadata(
        Dictionary<CredentialConfigurationId, SupportedCredentialConfiguration> credentialConfigurationsSupported,
        Option<List<IssuerDisplay>> display,
        Uri credentialEndpoint,
        CredentialIssuerId credentialIssuer,
        Option<IEnumerable<AuthorizationServerId>> authorizationServers)
    {
        CredentialConfigurationsSupported = credentialConfigurationsSupported;
        Display = display;
        CredentialEndpoint = credentialEndpoint;
        CredentialIssuer = credentialIssuer;
        AuthorizationServers = authorizationServers;
    }
    
    private static IssuerMetadata Create(
        Dictionary<CredentialConfigurationId, SupportedCredentialConfiguration> credentialConfigurationsSupported,
        Option<List<IssuerDisplay>> display,
        Uri credentialEndpoint,
        CredentialIssuerId credentialIssuer,
        Option<IEnumerable<AuthorizationServerId>> authorizationServers) => new(
        credentialConfigurationsSupported,
        display,
        credentialEndpoint,
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
            .Apply(credentialIssuerId)
            .Apply(authServers);
    }

    private const string CredentialConfigsSupportedJsonKey = "credential_configurations_supported";
    private const string DisplayJsonKey = "display";
    private const string CredentialEndpointJsonKey = "credential_endpoint";
    private const string CredentialIssuerJsonKey = "credential_issuer";
    private const string AuthorizationServersJsonKey = "authorization_servers";
}

public sealed class IssuerMetadataJsonConverter : JsonConverter<IssuerMetadata>
{
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, IssuerMetadata? value, JsonSerializer serializer) => 
        throw new NotImplementedException();

    public override IssuerMetadata ReadJson(
        JsonReader reader,
        Type objectType, 
        IssuerMetadata? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var json = JObject.Load(reader);
        
        var result = IssuerMetadata
            .ValidIssuerMetadata(json)
            .Match(
                metadata => metadata,
                errors => 
                    throw new InvalidOperationException($"IssuerMetadata is corrupt. Errors: {errors}")
            );
        
        return result;
    }
}
