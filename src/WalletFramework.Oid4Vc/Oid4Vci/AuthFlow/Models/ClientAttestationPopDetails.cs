using LanguageExt;
using static System.String;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

public record ClientAttestationPopDetails
{
    public string Audience { get; }
    
    public string Issuer { get; }
    
    public Option<string> Nonce { get; }
    
    private ClientAttestationPopDetails(string audience, string issuer, string? nonce)
    {
        Audience = audience;
        Issuer = issuer;
        Nonce = nonce;
    }

    public static ClientAttestationPopDetails CreateClientAttestationPopOptions(string audience, string issuer, string? nonce)
    {
        if (IsNullOrWhiteSpace(audience) || IsNullOrWhiteSpace(issuer))
        {
            throw new ArgumentException("Audience and Issuer must be provided.");
        }

        return new ClientAttestationPopDetails(audience, issuer, nonce);
    }
}
