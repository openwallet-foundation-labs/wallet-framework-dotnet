using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.ClaimMetadataJsonExtensions;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

public record ClaimMetadata
{
    public ClaimPath Path { get; }
    
    public Option<List<ClaimDisplay>> Display { get; }
    
    public Option<bool> Mandatory { get; }

    private ClaimMetadata(ClaimPath claimPath, Option<List<ClaimDisplay>> claimDisplays, Option<bool> mandatory)
    {
        Path = claimPath;
        Display = claimDisplays;
        Mandatory = mandatory;
    }
    
    private static ClaimMetadata Create(ClaimPath claimPath, Option<List<ClaimDisplay>> claimDisplays, Option<bool> mandatory) => 
        new(claimPath, claimDisplays, mandatory);
    
    public static Validation<ClaimMetadata> ValidClaimMetadata(JToken config, Func<JToken, Validation<ClaimPath>> claimPathValidation)
    {
        var claimPath =
            from jToken in config.GetByKey(PathJsonKey)
            from jArray in claimPathValidation(jToken)
            select jArray;

        var claimDisplays =
            from jToken in config.GetByKey(DisplayJsonKey)
            from jArray in jToken.ToJArray()
            from claimDisplay in jArray.TraverseAny(ClaimDisplay.ValidClaimDisplay)
            select claimDisplay.ToList();
        
        var mandatory =
            from jToken in config.GetByKey(MandatoryJsonKey)
            select jToken.ToObject<bool>();
        
        var result = ValidationFun.Valid(Create)
            .Apply(claimPath)
            .Apply(claimDisplays.ToOption())
            .Apply(mandatory.ToOption());

        return result;
    }
}

public static class ClaimMetadataJsonExtensions
{
    public const string PathJsonKey = "path";
    public const string DisplayJsonKey = "display";
    public const string MandatoryJsonKey = "mandatory";
    
    public static JObject EncodeToJson(this ClaimMetadata display)
    {
        var result = new JObject();

        result.Add(PathJsonKey, ClaimPath.ToJArray(display.Path));
        
        display.Display.IfSome(claimDisplays =>
        {
            var claimsArray = new JArray();
            foreach (var claimDisplay in claimDisplays)
            {
                claimsArray.Add(claimDisplay.EncodeToJson());
            }
            result.Add(DisplayJsonKey, claimsArray);
        });

        display.Mandatory.IfSome(mandatory => { result.Add(MandatoryJsonKey, mandatory); });

        return result;
    }
}
