using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Models;

public record DPopToken
{
    internal OAuthToken Token { get; }

    internal DPop DPop { get; }
    
    internal DPopToken(OAuthToken token, DPop dPop)
    {
        Token = token;
        DPop = dPop;
    }
}
