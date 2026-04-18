using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Oid4Vc.WalletAttestations.Issuance;

namespace WalletFramework.Oid4Vc.ClientAttestations.Abstractions;

public interface IAttestationSigner
{
    Task<string> CreateSignedJwt(object header, object payload, KeyId keyId);

    Task<SignedWalletAttestationRequest> SignWalletAttestationRequest(WalletAttestationRequest request);
}
