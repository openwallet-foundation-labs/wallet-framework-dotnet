using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;

namespace WalletFramework.Oid4Vc.ClientAttestations.Abstractions;

public interface IClientAttestationService
{
    public Task<Option<ClientAttestation>> GetClientAttestation(AuthorizationServerMetadata authorizationServerMetadata);
}
