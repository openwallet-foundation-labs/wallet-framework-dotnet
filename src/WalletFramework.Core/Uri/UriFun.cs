namespace WalletFramework.Core.Uri;

public static class UriFun
{
    public static string ToStringWithoutTrail(this System.Uri uri) => uri.ToString().TrimEnd('/');
}
