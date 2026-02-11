using WalletFramework.Core.Cryptography.Models;

namespace WalletFramework.Oid4Vc.ClientAttestations.Abstractions;

public interface IAttestationSigner
{
    Task<string> CreateSignedJwt(object header, object payload, KeyId keyId);
}
