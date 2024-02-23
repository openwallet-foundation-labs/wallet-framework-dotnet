using System.Net.Http;
using Hyperledger.Aries.Features.OpenID4VC.VCI.Models.DPop;

namespace Hyperledger.Aries.Features.OpenID4VC.VCI.Extensions
{
    internal static class Oid4VciHttpClientExtensions
    {
        public static void AddDPopHeader(this HttpClient httpClient, string dPopProofJwt)
        {
            httpClient.DefaultRequestHeaders.Remove("DPoP");
            httpClient.DefaultRequestHeaders.Add("DPoP", dPopProofJwt);
        }
        
        public static void AddAuthorizationHeader(this HttpClient httpClient, TokenResponseParameters tokenResponseParameters)
        {
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add(
                "Authorization",
                $"{tokenResponseParameters.TokenResponse.TokenType} {tokenResponseParameters.TokenResponse.AccessToken}"
                );
        }
    }
}
