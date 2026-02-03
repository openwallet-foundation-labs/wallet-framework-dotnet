namespace WalletFramework.Oid4Vc.WalletAttestations;

public static class WalletAttestationEndpoint
{
    public static Uri CreateWalletAttestationEndpoint(Uri root)
    {
        return new Uri(root, "wallet-instance/attestation");
    }
}
