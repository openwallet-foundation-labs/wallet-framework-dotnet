using WalletFramework.Oid4Vc.WalletAttestations;

namespace WalletFramework.Oid4Vc.ClientAttestations;

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

public static class ClientAttestationExtensions
{
    public static string ToStringRepresentation(this ClientAttestation clientAttestation)
        => clientAttestation.WalletAttestation.WalletAttestationJwt.EncodedToken + "~" +
           clientAttestation.WalletAttestationPopJwt;
}
