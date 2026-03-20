namespace WalletFramework.WalletAttestations;

public record ClientAttestationOptions
{
    public string ClientId { get; init; }

#pragma warning disable CS8618
    public ClientAttestationOptions()
    {
    }
#pragma warning restore CS8618

    public ClientAttestationOptions(string clientId)
    {
        ClientId = clientId;
    }
}
