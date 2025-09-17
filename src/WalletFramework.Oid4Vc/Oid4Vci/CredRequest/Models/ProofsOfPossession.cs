using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Models;

/// <summary>
///     Represents one or more proof of possessions of the key material that the issued credential is bound to.
///     This contains the jwts that acts as the proof of possessions.
/// </summary>
public record ProofsOfPossession(ProofTypeId ProofType, string[] Jwt);

public static class ProofsOfPossessionFun
{
    public static JObject EncodeToJson(this ProofsOfPossession proofsOfPossession)
    {
        return new JObject
        {
            [proofsOfPossession.ProofType.ToString()] = JArray.FromObject(proofsOfPossession.Jwt)
        };
    }
}
