using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.Extensions;

internal static class Oid4VciHttpClientExtensions
{
    public static HttpClient WithDPopHeader(this HttpClient httpClient, string dPopProofJwt)
    {
        httpClient.DefaultRequestHeaders.Remove("DPoP");
        httpClient.DefaultRequestHeaders.Add("DPoP", dPopProofJwt);

        return httpClient;
    }
    
    public static HttpClient WithAuthorizationHeader(this HttpClient httpClient, OAuthToken oAuthToken)
    {
        httpClient.DefaultRequestHeaders.Remove("Authorization");
        httpClient.DefaultRequestHeaders.Add(
            "Authorization",
            $"{oAuthToken.TokenType} {oAuthToken.AccessToken}");

        return httpClient;
    }
}
