using WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.DPoP
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
