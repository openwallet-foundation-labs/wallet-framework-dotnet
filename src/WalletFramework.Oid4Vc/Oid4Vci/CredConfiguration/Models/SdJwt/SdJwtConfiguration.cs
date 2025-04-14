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

        var order = config[OrderJsonName]?.ToObject<List<string>>();

        var claimToken = config[ClaimsJsonName];
        var claimMetadatas = claimToken switch
        {
            //Used to map the ListRepresentation from Vci Draft15 to DictionaryRepresentation of Draft14 and older
            JArray => ConvertToDictionaryRepresentation(claimToken.ToObject<List<ClaimMetadata>>()),
            JObject => claimToken.ToObject<Dictionary<string, ClaimMetadata>>(),
            _ => new Dictionary<string, ClaimMetadata>()
        };
        
        var result = ValidationFun.Valid(Create)
            .Apply(credentialConfiguration)
            .Apply(vct)
            .OnSuccess(configuration => configuration with
            {
                Claims = claimMetadatas,
                Order = order
            });

        return result;
    }

    private static Dictionary<string, ClaimMetadata> ConvertToDictionaryRepresentation(List<ClaimMetadata>? claimsV2)
    {
        var result = new Dictionary<string, ClaimMetadata>();

        if (claimsV2 == null)
            return result;
        
        foreach (var claim in claimsV2)
        {
            if (claim.Path == null || claim.Path.Count == 0)
                continue;

            AddToNestedClaims(result, claim.Path, claim);
        }

        return result;
    }

    private static void AddToNestedClaims(Dictionary<string, ClaimMetadata> currentLevel, List<string> path, ClaimMetadata sourceClaim)
    {
        var key = path[0];
        if (!currentLevel.TryGetValue(key, out var claimMetadata))
        {
            claimMetadata = new ClaimMetadata();
            currentLevel[key] = claimMetadata;
        }

        var isLeafClaim = path.Count == 1;
        if (isLeafClaim)
        {
            claimMetadata.Display = sourceClaim.Display;
            claimMetadata.ValueType = sourceClaim.ValueType;
            claimMetadata.Mandatory = sourceClaim.Mandatory;
        }
        else
        {
            var nextKey = path[1];

            if (claimMetadata.NestedClaims == null)
                claimMetadata.NestedClaims = new Dictionary<string, JToken>();

            if (!claimMetadata.NestedClaims.TryGetValue(nextKey, out var nextToken) || nextToken.Type != JTokenType.Object)
            {
                var childNode = new ClaimMetadata
                {
                    NestedClaims = new Dictionary<string, JToken>()
                };

                claimMetadata.NestedClaims[nextKey] = JObject.FromObject(childNode);
            }

            var nextNode = claimMetadata.NestedClaims[nextKey].ToObject<ClaimMetadata>()!;
            AddToNestedClaims(new Dictionary<string, ClaimMetadata> { [nextKey] = nextNode }, path.Skip(1).ToList(), sourceClaim);

            claimMetadata.NestedClaims[nextKey] = JObject.FromObject(nextNode);
        }
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
    
                if (claimMetadata.Value.NestedClaims == null || claimMetadata.Value.NestedClaims.Count == 0)
                    return claimMetadatas;
                
                foreach (var nested in claimMetadata.Value.NestedClaims!)
                {
                    claimMetadatas.Add(claimMetadata.Key + "." + nested.Key, nested.Value?.ToObject<ClaimMetadata>()!);
                }
    
                return claimMetadatas;
            })
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, ClaimMetadata>();
    }
}
