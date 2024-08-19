namespace WalletFramework.Oid4Vc.ClientAttestation;

public static class HttpClientExtensions
{
    public static void AddClientAttestationPopHeader(this HttpClient client, CombinedWalletAttestation clientAttestation)
    {
        client.DefaultRequestHeaders.Add("OAuth-Client-Attestation", clientAttestation.WalletInstanceAttestationJwt);
        client.DefaultRequestHeaders.Add("OAuth-Client-Attestation-PoP", clientAttestation.WalletInstanceAttestationPopJwt);
    }
}
