using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CryptograhicSigningAlgValue;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

/// <summary>
///     Represents proof type specific signing algorithm information.
/// </summary>
public record ProofTypeMetadata
{
    /// <summary>
    ///     Gets the available signing algorithms for the associated credential type.
    /// </summary>
    [JsonProperty("proof_signing_alg_values_supported")]
    public List<CryptograhicSigningAlgValue> ProofSigningAlgValuesSupported { get; }
    
    private ProofTypeMetadata(List<CryptograhicSigningAlgValue> proofSigningAlgValuesSupported)
    {
        ProofSigningAlgValuesSupported = proofSigningAlgValuesSupported;
    }

    public static Validation<ProofTypeMetadata> ValidProofTypeMetadata(JToken proofTypeMetadata) =>
        from jObject in proofTypeMetadata.ToJObject()
        from jToken in jObject.GetByKey("proof_signing_alg_values_supported")
        from jArray in jToken.ToJArray()
        from algValues in jArray.TraverseAny(ValidCryptograhicSigningAlgValue)
        select new ProofTypeMetadata(algValues.ToList());
}

public static class ProofTypeMetadataFun
{
    public static JObject EncodeToJson(this ProofTypeMetadata proofTypeMetadata)
    {
        var result = new JObject();

        var jArray = new JArray();
        foreach (var algValue in proofTypeMetadata.ProofSigningAlgValuesSupported)
        {
            jArray.Add(algValue.ToString());
        }
        result.Add("proof_signing_alg_values_supported", jArray);

        return result;
    }
}
