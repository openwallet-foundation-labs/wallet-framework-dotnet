namespace WalletFramework.Oid4Vci.Metadata;

internal static class MetadataDiscoveryUrl
{
    private const string CredentialIssuerWellKnownPath = "/.well-known/openid-credential-issuer";
    private const string AuthorizationServerWellKnownPath = "/.well-known/oauth-authorization-server";

    public static Uri ForCredentialIssuer(Uri issuerIdentifier) =>
        Create(issuerIdentifier, CredentialIssuerWellKnownPath);

    public static Uri ForAuthorizationServer(Uri issuerIdentifier) =>
        Create(issuerIdentifier, AuthorizationServerWellKnownPath);

    private static Uri Create(Uri issuerIdentifier, string wellKnownPath)
    {
        var issuerPath = issuerIdentifier.AbsolutePath;
        var metadataPath = string.IsNullOrWhiteSpace(issuerPath) || issuerPath == "/"
            ? wellKnownPath
            : $"{wellKnownPath}{issuerPath}";

        return new UriBuilder(issuerIdentifier)
        {
            Path = metadataPath,
            Query = string.Empty,
            Fragment = string.Empty
        }.Uri;
    }
}
