namespace WalletFramework.WalletAttestations;

public static class HttpClientExtensions
{
    public static void AddClientAttestation(this HttpClient client, ClientAttestation clientAttestation)
    {
        client.DefaultRequestHeaders.Remove("OAuth-Client-Attestation");
        client.DefaultRequestHeaders.Add("OAuth-Client-Attestation", clientAttestation.WalletAttestation.WalletAttestationJwt.EncodedToken);
        
        client.DefaultRequestHeaders.Remove("OAuth-Client-Attestation-PoP");
        client.DefaultRequestHeaders.Add("OAuth-Client-Attestation-PoP", clientAttestation.WalletAttestationPopJwt);
    }
}
