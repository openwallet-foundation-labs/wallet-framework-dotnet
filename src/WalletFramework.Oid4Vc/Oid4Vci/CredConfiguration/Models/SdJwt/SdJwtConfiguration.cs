using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.SdJwtVc.Models;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CredentialConfiguration;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.SdJwt.SdJwtConfiguration.SdJwtConfigurationJsonKeys;
using ClaimMetadata = WalletFramework.SdJwtVc.Models.Credential.Attributes.ClaimMetadata;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.SdJwt;

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

public static class SdJwtConfigurationFun
{
    public static JObject EncodeToJson(this SdJwtConfiguration config)
    {
        var credentialConfig = config.CredentialConfiguration.EncodeToJson();
        
        credentialConfig.Add(VctJsonName, config.Vct.ToString());

        if (config.Claims is not null)
        {
            credentialConfig.Add(ClaimsJsonName, JObject.FromObject(config.Claims));
        }

        if (config.Order is not null)
        {
            credentialConfig.Add(OrderJsonName, JArray.FromObject(config.Order));
        }
        
        return credentialConfig;
    }
    
    public static Dictionary<string, ClaimMetadata> ExtractClaimMetadata(this SdJwtConfiguration sdJwtConfiguration)
    {
        return sdJwtConfiguration
            .Claims?
            .Where(x => !string.IsNullOrWhiteSpace(x.Key) && x.Value.Display is not null)
            .SelectMany(claimMetadata => 
            {
                var claimMetadatas = new Dictionary<string, ClaimMetadata> { { claimMetadata.Key, claimMetadata.Value } };

                if (!(claimMetadata.Value.NestedClaims == null || claimMetadata.Value.NestedClaims.Count == 0))
                {
                    foreach (var nested in claimMetadata.Value.NestedClaims!)
                    {
                        claimMetadatas.Add(claimMetadata.Key + "." + nested.Key, nested.Value?.ToObject<ClaimMetadata>()!);
                    }
                }
                
                return claimMetadatas;
            })
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, ClaimMetadata>();
    }
}
