using LanguageExt;

namespace WalletFramework.WalletAttestations.Abstractions;

public interface IClientAttestationService
{
    public Task<Option<ClientAttestation>> GetClientAttestation(ClientAttestationRequest request);
}
