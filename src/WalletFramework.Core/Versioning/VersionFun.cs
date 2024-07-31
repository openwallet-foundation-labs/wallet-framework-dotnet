namespace WalletFramework.Core.Versioning;

public static class VersionFun
{
    public static string ToMajorMinorString(this Version version)
    {
        var major = version.Major.ToString();
        var minor = version.Minor.ToString();

        return major + "." + minor;
    }
}
