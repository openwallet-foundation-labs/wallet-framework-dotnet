
using WalletFramework.Core.Cryptography.Models;

namespace WalletFramework.MdocLib.Security.Cose.Abstractions;

public interface ICoseSign1Signer
{
    Task<CoseSignature> Sign(SigStructure sigStructure, KeyId keyId);
}
