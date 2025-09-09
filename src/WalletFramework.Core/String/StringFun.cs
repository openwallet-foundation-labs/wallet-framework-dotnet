namespace WalletFramework.Core.String;

public static class StringFun
{
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);
    
    public static byte[] GetUtf8Bytes(this string value) => System.Text.Encoding.UTF8.GetBytes(value);
}
