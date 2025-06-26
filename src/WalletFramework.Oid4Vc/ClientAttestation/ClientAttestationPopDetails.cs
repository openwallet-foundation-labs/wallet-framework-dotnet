using LanguageExt;
using static System.String;

namespace WalletFramework.Oid4Vc.ClientAttestation;

public record ClientAttestationPopDetails
{
    public string ClientId { get; }
    
    public Option<string> Nonce { get; }
    
    private ClientAttestationPopDetails(string clientId, string? nonce)
    {
        ClientId = clientId;
        Nonce = nonce;
    }

    public static ClientAttestationPopDetails CreateClientAttestationPopOptions(string clientId = "https://oob.lissi.io", string? nonce = null)
    {
        if (IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentException("Audience and Issuer must be provided.");
        }

        return new ClientAttestationPopDetails(clientId, nonce);
    }
}
