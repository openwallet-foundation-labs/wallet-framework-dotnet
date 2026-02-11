namespace WalletFramework.Oid4Vc.WalletAttestations;

public readonly record struct WalletInstanceId(string Value)
{
    public static implicit operator string(WalletInstanceId id) => id.Value;
}
