using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.Oid4Vc.ClientAttestation;

public struct WalletInstanceAttestationJwt
{
    public string Value { get; }
    
    public static implicit operator string(WalletInstanceAttestationJwt keyId) => keyId.Value;
    
    private WalletInstanceAttestationJwt(string value) => Value = value;

    public static Validation<WalletInstanceAttestationJwt> ValidWalletInstanceAttestationJwt(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new StringIsNullOrWhitespaceError<WalletInstanceAttestationJwt>();
        }
        
        return new WalletInstanceAttestationJwt(value);
    }
}
