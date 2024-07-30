namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

public struct WalletInstanceAttestationPopJwt
{
    public string Value { get; }
    
    public static implicit operator string(WalletInstanceAttestationPopJwt keyId) => keyId.Value;
    
    private WalletInstanceAttestationPopJwt(string value) => Value = value;

    public static WalletInstanceAttestationPopJwt CreateWalletInstanceAttestationPopJwt(string value) => new (value);
}
