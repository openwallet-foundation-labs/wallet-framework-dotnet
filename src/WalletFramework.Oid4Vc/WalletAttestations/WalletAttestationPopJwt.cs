namespace WalletFramework.Oid4Vc.WalletAttestations;

public struct WalletAttestationPopJwt
{
    public string Value { get; }
    
    public static implicit operator string(WalletAttestationPopJwt keyId) => keyId.Value;
    
    private WalletAttestationPopJwt(string value) => Value = value;

    public static WalletAttestationPopJwt Create(string value) => new (value);
}
