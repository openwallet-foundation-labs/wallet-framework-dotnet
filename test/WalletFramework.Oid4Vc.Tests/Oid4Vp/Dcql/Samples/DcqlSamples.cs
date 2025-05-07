using System.Reflection;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.Samples;

public static class DcqlSamples
{
    public static string GetDcqlQueryAsJsonStr() => GetJsonForTestCase("DcqlQuerySample");

    private static string GetJsonForTestCase(string name = "")
    {
        var assembly = Assembly.GetExecutingAssembly();
        var currentNamespace = typeof(DcqlSamples).Namespace;
        var resourceName = $"{currentNamespace}.{name}.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Could not find resource with name {resourceName}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
