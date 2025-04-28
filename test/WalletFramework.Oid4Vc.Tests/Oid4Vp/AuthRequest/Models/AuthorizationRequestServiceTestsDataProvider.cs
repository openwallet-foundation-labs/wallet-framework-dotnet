using System.Reflection;
using System.Runtime.CompilerServices;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.AuthRequest.Models;

public static class AuthorizationRequestServiceTestsDataProvider
{
    public static string GetJsonForTestCase([CallerMemberName]string name = "")
    {
        var assembly = Assembly.GetExecutingAssembly();
        var currentNamespace = typeof(AuthorizationRequestServiceTestsDataProvider).Namespace;
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
