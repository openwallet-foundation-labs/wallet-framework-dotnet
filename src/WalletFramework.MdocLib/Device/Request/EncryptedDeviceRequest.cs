using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Device.Abstractions;
using WalletFramework.MdocLib.Device.Request.Errors;

namespace WalletFramework.MdocLib.Device.Request;

public record EncryptedDeviceRequest(DeviceRequest Decrypted, byte[] Encrypted)
{
    public static Validation<EncryptedDeviceRequest> FromBytes(byte[] encrypted, byte[] derivatedKey, IAesGcmEncryption aes)
    {
        byte[] decrypted;
        try
        {
            decrypted = aes.Decrypt(
                encrypted,
                derivatedKey,
                false);
        }
        catch (Exception e)
        {
            return new DecryptionError<EncryptedDeviceRequest>(e);
        }
        
        var cbor = CBORObject.DecodeFromBytes(decrypted);
        return 
            from deviceRequest in DeviceRequest.FromCbor(cbor)
            select new EncryptedDeviceRequest(deviceRequest, encrypted);
    }
}

public static class EncryptedDeviceRequestFun
{
    public static EncryptedDeviceRequest Encrypt(this DeviceRequest deviceRequest, byte[] key, IAesGcmEncryption aes)
    {
        var plainText = deviceRequest.ToCbor().EncodeToBytes();
        
        var encryptedData = aes.Encrypt(
            plainText,
            key,
            false);

        return new EncryptedDeviceRequest(deviceRequest, encryptedData);
    }
}
