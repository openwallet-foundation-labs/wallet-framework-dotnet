using WalletFramework.Core.Cryptography.Models;
using WalletFramework.MdocLib.Device;

namespace WalletFramework.MdocLib.Security.Cose.Abstractions;

public interface ICoseSign1Signer
{
    /// <remarks>Only works for DeviceAuthenticationBytes currently,
    /// we can make it work for general byte strings if needed</remarks>
    Task<CoseSignature> Sign(DeviceAuthentication deviceAuthentication, KeyId keyId);
}
