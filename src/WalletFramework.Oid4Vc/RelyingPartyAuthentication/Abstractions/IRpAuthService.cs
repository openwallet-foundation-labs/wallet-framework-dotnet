using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.Abstractions;

public interface IRpAuthService
{
    public Task<RpAuthResult> Authenticate(RequestObject requestObject);
}
