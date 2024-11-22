using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using WalletFramework.MdocLib.Device.Abstractions;

namespace WalletFramework.MdocLib.Device.Implementations;

public class AesGcmEncryption : IAesGcmEncryption
{
    // Set the identifier based on the role (mdoc Reader or mdoc App)
    private uint _messageCounter = 1; // Initialize counter to 1 as per spec

    private byte[] GetIdentifier(bool isReader)
    {
        return isReader ? new byte[8] : [0, 0, 0, 0, 0, 0, 0, 1];
    }
    
    public byte[] Decrypt(byte[] encryptedData, byte[] cek, bool isReader)
    {
        var iv = GenerateIv(isReader);
        
        // Split the encryptedData into cipherText and authTag
        int authTagLength = 16;
        var cipherText = new byte[encryptedData.Length - authTagLength];
        var authTag = new byte[authTagLength];

        Array.Copy(encryptedData, 0, cipherText, 0, cipherText.Length);
        Array.Copy(encryptedData, cipherText.Length, authTag, 0, authTag.Length);

        // Initialize the GCM block cipher for decryption
        var gcmBlockCipher = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(cek), 128, iv, []); // Empty AAD
        gcmBlockCipher.Init(false, parameters);

        // Combine cipherText and authTag to match GCM's expected input format
        var combinedCipherText = cipherText.Concat(authTag).ToArray();

        var plainText = new byte[gcmBlockCipher.GetOutputSize(combinedCipherText.Length)];
        var len = gcmBlockCipher.ProcessBytes(combinedCipherText, 0, combinedCipherText.Length, plainText, 0);
        gcmBlockCipher.DoFinal(plainText, len);
        
        IncrementCounter();

        return plainText;
    }

    public byte[] Encrypt(byte[] plainText, byte[] cek, bool isReader)
    {
        var iv = GenerateIv(isReader);
        
        var gcmBlockCipher = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(cek), 128, iv, []); // Empty AAD
        gcmBlockCipher.Init(true, parameters);

        var cipherText = new byte[gcmBlockCipher.GetOutputSize(plainText.Length)];
        var len = gcmBlockCipher.ProcessBytes(plainText, 0, plainText.Length, cipherText, 0);
        gcmBlockCipher.DoFinal(cipherText, len);

        var authTag = new byte[16];
        Array.Copy(cipherText, cipherText.Length - authTag.Length, authTag, 0, authTag.Length);

        var actualCipherText = new byte[cipherText.Length - authTag.Length];
        Array.Copy(cipherText, 0, actualCipherText, 0, actualCipherText.Length);

        IncrementCounter();

        return actualCipherText.Concat(authTag).ToArray();
    }

    public void ResetMessageCounter()
    {
        _messageCounter = 1;
    }

    private byte[] GenerateIv(bool isReader)
    {
        // TODO: This is hardcoded
        
        // var iv = new byte[12];
        //
        // var identifier = new byte[8];
        // uint messageCounter = 1;
        //
        // Array.Copy(identifier, iv, identifier.Length);
        //
        // // Convert message counter to big-endian byte array
        // var counterBytes = BitConverter.GetBytes(messageCounter);
        // if (BitConverter.IsLittleEndian)
        //     Array.Reverse(counterBytes);
        //
        // Array.Copy(counterBytes, 0, iv, identifier.Length, counterBytes.Length);
        // return iv;

        var identifier = GetIdentifier(isReader);
        
        var iv = new byte[12];
        Array.Copy(identifier, iv, identifier.Length);
        
        // Convert message counter to big-endian byte array
        var counterBytes = BitConverter.GetBytes(_messageCounter);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(counterBytes);
        
        Array.Copy(counterBytes, 0, iv, identifier.Length, counterBytes.Length);
        return iv;
    }

    private void IncrementCounter()
    {
        _messageCounter++;
    }
}
