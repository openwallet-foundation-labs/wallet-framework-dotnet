using OneOf;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.ClientAttestation;

public interface IClientAttestationService
{
    public Task<CombinedWalletAttestation> GetCombinedWalletAttestationAsync(ClientAttestationDetails clientAttestationDetails, OneOf<AuthorizationServerMetadata, AuthorizationRequest> audienceSource);
}
