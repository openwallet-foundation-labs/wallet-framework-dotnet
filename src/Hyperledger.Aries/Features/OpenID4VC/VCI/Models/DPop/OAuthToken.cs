using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Authorization;

namespace Hyperledger.Aries.Features.OpenID4VC.VCI.Models.DPop
{
    internal record OAuthToken
    {
        internal OAuthToken(TokenResponse tokenResponse, DPop? dPop = null)
        {
            TokenResponse = tokenResponse;
            DPop = dPop;
        }

        public TokenResponse TokenResponse { get; }
        
        public DPop? DPop { get; }
    }

    internal record DPop(string KeyId, string? Nonce)
    {
        public string? Nonce { get; } = Nonce;

        public string KeyId { get; } = KeyId;
    }
    
    internal static class OAuthTokenExtensions
    {
        internal static bool IsDPoPRequested(this OAuthToken oAuthToken)
        {
            return oAuthToken.TokenResponse.TokenType == "DPoP"
                   && oAuthToken.DPop != null;
        }
    }
}
