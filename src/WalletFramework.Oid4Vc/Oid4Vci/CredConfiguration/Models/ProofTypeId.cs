using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

public readonly struct ProofTypeId
{
    private const string JwtProofTypeId = "jwt";
    
    private string Value { get; }
    
    private ProofTypeId(string value)
    {
        Value = value;
    }

    public string AsString() => Value;
    
    public override string ToString() => Value;

    public static implicit operator string(ProofTypeId proofTypeId) => proofTypeId.ToString();

    public static Validation<ProofTypeId> ValidProofTypeId(JToken proofTypeId) => proofTypeId.ToJValue().OnSuccess(value =>
    {
        var str = value.ToString(CultureInfo.InvariantCulture);
        return SupportedProofTypes.Contains(str)
            ? new ProofTypeId(str)
            : new ProofTypeIdNotSupportedError(str).ToInvalid<ProofTypeId>();
    });

    public static ProofTypeId GetJwtProofTypeId() =>
        new (JwtProofTypeId);
    
    private static List<string> SupportedProofTypes => new()
    {
        JwtProofTypeId
    };
}
