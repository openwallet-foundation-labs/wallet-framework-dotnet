using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.MdocLib;
using WalletFramework.SdJwtVc.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record PresentedCredentialSet
{
    public CredentialSetId CredentialSetId { get; set; } = null!;
    
    public Option<Vct> SdJwtCredentialType { get; set; }
    
    public Option<DocType> MDocCredentialType { get; set; }

    public Dictionary<string, PresentedClaim> PresentedClaims { get; set; } = null!;
}

public static class PresentedCredentialSetExtensions
{
    private const string CredentialSetIdJsonKey = "credential_set_id";
    private const string SdJwtCredentialTypeJsonKey = "sd_jwt_credential_type";
    private const string MDocCredentialTypeJsonKey = "mdoc_credential_type";
    private const string PresentedClaimsJsonKey = "presented_claims";
    
    public static JObject EncodeToJson(this PresentedCredentialSet presentedCredentialSet)
    {
        var result = new JObject();
        
        result.Add(CredentialSetIdJsonKey, presentedCredentialSet.CredentialSetId.ToString());

        presentedCredentialSet.SdJwtCredentialType.IfSome(sdJwtType =>
            result.Add(SdJwtCredentialTypeJsonKey, sdJwtType.ToString()));
        
        presentedCredentialSet.MDocCredentialType.IfSome(mDocType =>
            result.Add(MDocCredentialTypeJsonKey, mDocType.ToString()));
        
        var jObjectDictionary = new JObject();
        foreach (var kvp in presentedCredentialSet.PresentedClaims)
        {
            jObjectDictionary.Add(kvp.Key, JsonConvert.SerializeObject(kvp.Value));
        }
        result.Add(PresentedClaimsJsonKey, jObjectDictionary);
    
        return result;
    }
    
    public static List<PresentedCredentialSet> DecodeFromJson(JArray jArray) =>
        jArray.TraverseAll(token => 
            from jObject in token.ToJObject()
            select DecodeFromJson(jObject)
        ).UnwrapOrThrow().ToList();
    
    private static PresentedCredentialSet DecodeFromJson(JObject jObject)
    {
        var credentialSetId = jObject[CredentialSetIdJsonKey]!.ToString();
        
        var sdJwtCredentialType = 
            from jToken in jObject.GetByKey(SdJwtCredentialTypeJsonKey).ToOption()
            from vct in Vct.ValidVct(jToken).ToOption()
            select vct;
        
        var mDocCredentialType =
            from jToken in jObject.GetByKey(MDocCredentialTypeJsonKey).ToOption()
            from docType in DocType.ValidDoctype(jToken).ToOption()
            select docType;
        
        var presentedClaims = 
            from jToken in jObject.GetByKey(PresentedClaimsJsonKey).ToOption()
            from claimsJson in jToken.ToJObject().ToOption()
            from displays in claimsJson.Properties().Select(prop => (
                    ClaimName: prop.Name,
                    ClaimValue: JsonConvert.DeserializeObject<PresentedClaim>(prop.Value.ToString())
                ))
            select displays;
        
        return new PresentedCredentialSet
        {
            CredentialSetId = CredentialSetId .ValidCredentialSetId(credentialSetId).UnwrapOrThrow(),
            SdJwtCredentialType = sdJwtCredentialType,
            MDocCredentialType = mDocCredentialType,
            PresentedClaims = presentedClaims.ToDictionary(kvp => kvp.ClaimName, kvp => kvp.ClaimValue)
        };
    }
}
