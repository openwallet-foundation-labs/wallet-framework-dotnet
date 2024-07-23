using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

public record CombinedWalletAttestation
{
    public WalletInstanceAttestationJwt WalletInstanceAttestationJwt { get; }
    
    public WalletInstanceAttestationPopJwt WalletInstanceAttestationPopJwt { get; }

    private CombinedWalletAttestation(
        WalletInstanceAttestationJwt walletInstanceAttestationJwt, 
        WalletInstanceAttestationPopJwt walletInstanceAttestationPopJwt)
    {
        WalletInstanceAttestationJwt = walletInstanceAttestationJwt;
        WalletInstanceAttestationPopJwt = walletInstanceAttestationPopJwt;
    }

    public static CombinedWalletAttestation Create(
        WalletInstanceAttestationJwt walletInstanceAttestationJwt, 
        WalletInstanceAttestationPopJwt walletInstanceAttestationPopJwt)
        => new(walletInstanceAttestationJwt, walletInstanceAttestationPopJwt);
}

public static class CombinedWalletAttestationExtensions
{
    public static string ToStringRepresentation(this CombinedWalletAttestation combinedWalletAttestation)
        => combinedWalletAttestation.WalletInstanceAttestationJwt + "~" +
           combinedWalletAttestation.WalletInstanceAttestationPopJwt;
}

public struct WalletInstanceAttestationPopJwt
{
    public string Value { get; }
    
    public static implicit operator string(WalletInstanceAttestationPopJwt keyId) => keyId.Value;
    
    private WalletInstanceAttestationPopJwt(string value) => Value = value;

    public static WalletInstanceAttestationPopJwt CreateWalletInstanceAttestationPopJwt(string value) => new (value);
}

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
