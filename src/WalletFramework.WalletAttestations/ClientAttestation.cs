namespace WalletFramework.WalletAttestations;

public record ClientAttestation
{
    public WalletAttestation WalletAttestation { get; }

    public WalletAttestationPopJwt WalletAttestationPopJwt { get; }

    private ClientAttestation(
        WalletAttestation walletAttestation,
        WalletAttestationPopJwt walletAttestationPopJwt)
    {
        WalletAttestation = walletAttestation;
        WalletAttestationPopJwt = walletAttestationPopJwt;
    }

    public static ClientAttestation Create(
        WalletAttestation walletAttestation,
        WalletAttestationPopJwt walletAttestationPopJwt)
        => new(walletAttestation, walletAttestationPopJwt);
}
