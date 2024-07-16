using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.SdJwtVc.Models.Credential.Attributes;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CredentialConfiguration;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.SdJwt.SdJwtConfiguration.SdJwtConfigurationJsonKeys;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.SdJwt;

[JsonConverter(typeof(SdJwtConfigurationJsonConverter))]
public record SdJwtConfiguration
{
    public CredentialConfiguration CredentialConfiguration { get; }

    public Format Format => CredentialConfiguration.Format;
    
    public Vct Vct { get; }
    
    /// <summary>
    ///     Gets or sets the dictionary representing the attributes of the credential in different languages.
    /// </summary>
    public Dictionary<string, ClaimMetadata>? Claims { get; set; }
    
    /// <summary>
    ///     A list of claim display names, arranged in the order in which they should be displayed by the Wallet.
    /// </summary>
    public List<string>? Order { get; set; }
    
    private SdJwtConfiguration(CredentialConfiguration credentialConfiguration, Vct vct)
    {
        CredentialConfiguration = credentialConfiguration;
        Vct = vct;
    }
    
    private static SdJwtConfiguration Create(CredentialConfiguration credentialConfiguration, Vct vct) => 
        new(credentialConfiguration, vct);
    
    public static Validation<SdJwtConfiguration> ValidSdJwtCredentialConfiguration(JToken config)
    {
        var credentialConfiguration = ValidCredentialConfiguration(config);
        var vct = config.GetByKey(VctJsonName).OnSuccess(Vct.ValidVct);
        
        var claims = config[ClaimsJsonName]?.ToObject<Dictionary<string, ClaimMetadata>>();
        var order = config[OrderJsonName]?.ToObject<List<string>>();

        var result = ValidationFun.Valid(Create)
            .Apply(credentialConfiguration)
            .Apply(vct)
            .OnSuccess(configuration => configuration with
            {
                Claims = claims,
                Order = order
            });

        return result;
    }

    public static class SdJwtConfigurationJsonKeys
    {
        public const string VctJsonName = "vct";
        public const string ClaimsJsonName = "claims";
        public const string OrderJsonName = "order";
    }
}

public class SdJwtConfigurationJsonConverter : JsonConverter<SdJwtConfiguration>
{
    public override bool CanRead => false;

    public override void WriteJson(JsonWriter writer, SdJwtConfiguration? sdJwtConfig, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        var credentialConfig = JObject.FromObject(sdJwtConfig!.CredentialConfiguration, serializer);
        foreach (var property in credentialConfig.Properties())
        {
            property.WriteTo(writer);
        }

        writer.WritePropertyName(ClaimsJsonName);
        serializer.Serialize(writer, sdJwtConfig.Claims);
        
        writer.WritePropertyName(OrderJsonName);
        serializer.Serialize(writer, sdJwtConfig.Order);
        
        writer.WritePropertyName(VctJsonName);
        serializer.Serialize(writer, sdJwtConfig.Vct);

        writer.WriteEndObject(); 
    }

    public override SdJwtConfiguration ReadJson(JsonReader reader, Type objectType, SdJwtConfiguration? existingValue,
        bool hasExistingValue, JsonSerializer serializer) =>
        throw new NotImplementedException();
}
