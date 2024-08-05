using WalletFramework.Core.Cryptography.Models;
using WalletFramework.MdocLib.Device.Abstractions;
using WalletFramework.MdocLib.Security;
using WalletFramework.MdocLib.Security.Cose.Abstractions;
using static WalletFramework.MdocLib.Security.Cose.ProtectedHeaders;

namespace WalletFramework.MdocLib.Device.Implementations;

public class MdocAuthenticationService : IMdocAuthenticationService
{
    public MdocAuthenticationService(ICoseSign1Signer coseSign1Signer)
    {
        _coseSign1Signer = coseSign1Signer;
    }

    private readonly ICoseSign1Signer _coseSign1Signer;
    
    public async Task<AuthenticatedMdoc> Authenticate(Mdoc mdoc, SessionTranscript sessionTranscript, KeyId keyId)
    {
        // TODO: Unsure how device namespaces should be built
        var deviceNamespaces = mdoc.IssuerSigned.IssuerNameSpaces.ToDeviceNameSpaces();
        var deviceAuthentication = new DeviceAuthentication(sessionTranscript, mdoc.DocType, deviceNamespaces);
        
        var signature = await _coseSign1Signer.Sign(deviceAuthentication, keyId);
        
        var deviceSignature = new DeviceSignature(BuildProtectedHeaders(), signature);
        var deviceAuth = new DeviceAuth(deviceSignature);
        var deviceSigned = new DeviceSigned(deviceNamespaces, deviceAuth);

        return new AuthenticatedMdoc(mdoc, deviceSigned);
    }
}
