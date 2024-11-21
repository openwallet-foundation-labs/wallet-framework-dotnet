using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Device.Abstractions;
using WalletFramework.MdocLib.Device.Request.Errors;

namespace WalletFramework.MdocLib.Device.Response;

public record EncryptedDeviceResponse(DeviceResponse Decrypted, byte[] Encrypted)
{
    public static Validation<EncryptedDeviceResponse> FromBytes(byte[] encrypted, byte[] derivatedKey,
        IAesGcmEncryption aes)
    {
        byte[] decrypted;
        try
        {
            decrypted = aes.Decrypt(
                encrypted,
                derivatedKey,
                true);
        }
        catch (Exception e)
        {
            return new DecryptionError<EncryptedDeviceResponse>(e);
        }
        
        var cbor = CBORObject.DecodeFromBytes(decrypted);
        return 
            from deviceResponse in DeviceResponseFun.FromCbor(cbor)
            select new EncryptedDeviceResponse(deviceResponse, encrypted);
    }
}

public static class EncryptedDeviceResponseFun
{
    public static EncryptedDeviceResponse Encrypt(this DeviceResponse response, byte[] key,
        IAesGcmEncryption aes)
    {
        var plainText = response.ToCbor().EncodeToBytes();
        
        var encryptedData = aes.Encrypt(
            plainText,
            key,
            true);

        return new EncryptedDeviceResponse(response, encryptedData);
    }
}
