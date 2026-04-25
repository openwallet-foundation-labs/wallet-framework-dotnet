using LanguageExt;

namespace WalletFramework.WalletAttestations.Abstractions;

public interface IWalletAttestationService
{
    /// <summary>
    ///     Request new wallet instance attestation from the attestation service.
    /// </summary>
    /// <returns>
    ///     The wallet attestation.
    /// </returns>
    Task<Option<WalletAttestation>> RequestWalletAttestation();
}
