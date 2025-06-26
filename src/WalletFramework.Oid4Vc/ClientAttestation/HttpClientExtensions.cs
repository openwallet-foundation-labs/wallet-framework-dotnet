namespace WalletFramework.Oid4Vc.ClientAttestation;

public static class HttpClientExtensions
{
    public static void AddClientAttestationPopHeader(this HttpClient client, CombinedWalletAttestation clientAttestation)
    {
        client.DefaultRequestHeaders.Remove("OAuth-Client-Attestation");
        client.DefaultRequestHeaders.Add("OAuth-Client-Attestation", clientAttestation.WalletInstanceAttestationJwt);
        
        client.DefaultRequestHeaders.Remove("OAuth-Client-Attestation-PoP");
        client.DefaultRequestHeaders.Add("OAuth-Client-Attestation-PoP", clientAttestation.WalletInstanceAttestationPopJwt);
    }
}
