using Newtonsoft.Json.Linq;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.SdJwtVc.Models;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CredentialConfiguration;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.SdJwt.SdJwtConfiguration.SdJwtConfigurationJsonKeys;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.SdJwt;

public record SdJwtConfiguration
{
    public CredentialConfiguration CredentialConfiguration { get; }

    public Format Format => CredentialConfiguration.Format;
    
    public Vct Vct { get; }
    
    private SdJwtConfiguration(CredentialConfiguration credentialConfiguration, Vct vct)
    {
        CredentialConfiguration = credentialConfiguration;
        Vct = vct;
    }
    
    private static SdJwtConfiguration Create(CredentialConfiguration credentialConfiguration, Vct vct) => 
        new(credentialConfiguration, vct);
    
    public static Validation<SdJwtConfiguration> ValidSdJwtCredentialConfiguration(JToken config)
    {
        var sdJwtClaimPathValidation = new Func<JToken, Validation<ClaimPath>>(token =>
        {
            var sdJwtClaimPath =
                from jArray in token.ToJArray()
                from validClaimPath in ClaimPath.FromJArray(jArray)
                select validClaimPath;

            return sdJwtClaimPath;
        });
        
        var credentialConfiguration = ValidCredentialConfiguration(config, sdJwtClaimPathValidation);
        var vct = config.GetByKey(VctJsonName).OnSuccess(Vct.ValidVct);

        var result = ValidationFun.Valid(Create)
            .Apply(credentialConfiguration)
            .Apply(vct);

        return result;
    }
    
    public static class SdJwtConfigurationJsonKeys
    {
        public const string VctJsonName = "vct";
    }
}

public static class SdJwtConfigurationFun
{
    public static JObject EncodeToJson(this SdJwtConfiguration config)
    {
        var credentialConfig = config.CredentialConfiguration.EncodeToJson();
        
        credentialConfig.Add(VctJsonName, config.Vct.ToString());
        
        return credentialConfig;
    }
}
