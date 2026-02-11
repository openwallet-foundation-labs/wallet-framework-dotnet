using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.WalletAttestations.Abstractions;

public interface IWalletAttestationService
{
    /// <summary>
    ///     Request new wallet instance attestation from the attestation service.
    /// </summary>
    /// <returns>
    ///     The wallet attestation.
    /// </returns>
    Task<Validation<WalletAttestation>> RequestWalletAttestation();
}
