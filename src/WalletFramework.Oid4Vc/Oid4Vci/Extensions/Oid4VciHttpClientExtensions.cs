using WalletFramework.Oid4Vc.Oid4Vci.Models.DPop;

namespace WalletFramework.Oid4Vc.Oid4Vci.Extensions
{
    internal static class Oid4VciHttpClientExtensions
    {
        public static void AddDPopHeader(this HttpClient httpClient, string dPopProofJwt)
        {
            httpClient.DefaultRequestHeaders.Remove("DPoP");
            httpClient.DefaultRequestHeaders.Add("DPoP", dPopProofJwt);
        }
        
        public static void AddAuthorizationHeader(this HttpClient httpClient, OAuthToken oAuthToken)
        {
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add(
                "Authorization",
                $"{oAuthToken.TokenResponse.TokenType} {oAuthToken.TokenResponse.AccessToken}"
                );
        }
    }
}
