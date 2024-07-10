using static System.String;

namespace WalletFramework.SdJwtVc.Models;

public record ClientAttestationPopOptions
{
    public string Audience { get; }
    public string Issuer { get; }
    //TODO: Change nullable to Option<> when available
    public string? Nonce { get; }
    
    private ClientAttestationPopOptions(string audience, string issuer, string? nonce)
    {
        Audience = audience;
        Issuer = issuer;
        Nonce = nonce;
    }

    public static ClientAttestationPopOptions CreateClientAttestationPopOptions(string audience, string issuer, string? nonce)
    {
        if (IsNullOrWhiteSpace(audience) || IsNullOrWhiteSpace(issuer))
        {
            throw new ArgumentException("Audience and Issuer must be provided.");
        }

        return new ClientAttestationPopOptions(audience, issuer, nonce);
    }
}
