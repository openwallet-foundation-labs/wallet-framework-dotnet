namespace WalletFramework.Oid4Vc.ClientAttestation;

public struct WalletInstanceAttestationPopJwt
{
    public string Value { get; }
    
    public static implicit operator string(WalletInstanceAttestationPopJwt keyId) => keyId.Value;
    
    private WalletInstanceAttestationPopJwt(string value) => Value = value;

    public static WalletInstanceAttestationPopJwt Create(string value) => new (value);
}
