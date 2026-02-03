using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.WalletAttestations;

namespace WalletFramework.Oid4Vc.ClientAttestations.Abstractions;

public interface IClientAttestationService
{
    public Task<ClientAttestation> GetClientAttestation(AuthorizationServerMetadata authorizationServerMetadata);
}
