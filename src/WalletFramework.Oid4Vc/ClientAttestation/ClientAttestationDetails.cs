using WalletFramework.Core.Cryptography.Models;

namespace WalletFramework.Oid4Vc.ClientAttestation;

public record ClientAttestationDetails()
{
    public WalletInstanceAttestationJwt WalletInstanceAttestationJwt { get; init; }

    public KeyId AssociatedKeyId { get; init; }
    
    public ClientAttestationPopDetails ClientAttestationPopDetails { get; init; }

    public static ClientAttestationDetails Create(
        WalletInstanceAttestationJwt walletInstanceAttestationJwt,
        KeyId associatedKeyId,
        ClientAttestationPopDetails clientAttestationPopDetails)
        => new()
        {
            WalletInstanceAttestationJwt = walletInstanceAttestationJwt,
            AssociatedKeyId = associatedKeyId,
            ClientAttestationPopDetails = clientAttestationPopDetails
        };
}
