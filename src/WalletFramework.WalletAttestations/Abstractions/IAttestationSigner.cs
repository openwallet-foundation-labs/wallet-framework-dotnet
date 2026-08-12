using WalletFramework.Core.Cryptography.Models;
using WalletFramework.WalletAttestations.Issuance;

namespace WalletFramework.WalletAttestations.Abstractions;

public interface IAttestationSigner
{
    Task<string> CreateSignedJwt(object header, object payload, KeyId keyId);

    Task<SignedWalletAttestationRequest> SignWalletAttestationRequest(WalletAttestationRequest request);
}
