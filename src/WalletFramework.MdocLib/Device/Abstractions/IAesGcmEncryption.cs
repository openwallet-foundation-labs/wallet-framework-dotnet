namespace WalletFramework.MdocLib.Device.Abstractions;

public interface IAesGcmEncryption
{
    byte[] Decrypt(byte[] encryptedData, byte[] cek, bool isReader);

    byte[] Encrypt(byte[] plainText, byte[] cek, bool isReader);

    void ResetMessageCounter();
}
