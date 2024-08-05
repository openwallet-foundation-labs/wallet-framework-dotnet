using WalletFramework.Core.Cryptography.Abstractions;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.MdocLib.Device;
using WalletFramework.MdocLib.Security.Cose.Abstractions;

namespace WalletFramework.MdocLib.Security.Cose.Implementations;

public class CoseSign1Signer : ICoseSign1Signer
{
    public CoseSign1Signer(IKeyStore keyStore)
    {
        _keyStore = keyStore;
    }
    
    private readonly IKeyStore _keyStore;
    
    public async Task<CoseSignature> Sign(DeviceAuthentication deviceAuthentication, KeyId keyId)
    {
        var sigStructure = new SigStructure(deviceAuthentication).ToCborByteString();
        var signature = await _keyStore.Sign(keyId, sigStructure.EncodedBytes);
        return new CoseSignature(signature);
    }
}
