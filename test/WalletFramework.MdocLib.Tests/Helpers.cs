using System.Security.Cryptography;

namespace WalletFramework.MdocLib.Tests;

public static class Helpers
{
    public static byte[] GenerateRandomBytes()
    {
        var byteArray = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(byteArray);
        return byteArray;
    }
}
