using WalletFramework.Core.Cryptography.Models;
using WalletFramework.MdocLib.Security;

namespace WalletFramework.MdocLib.Device.Abstractions;

public interface IMdocAuthenticationService
{
    Task<AuthenticatedMdoc> Authenticate(Mdoc mdoc, SessionTranscript sessionTranscript, KeyId keyId);
}
