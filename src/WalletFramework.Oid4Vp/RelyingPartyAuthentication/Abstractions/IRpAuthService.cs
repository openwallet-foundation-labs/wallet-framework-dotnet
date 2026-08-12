using WalletFramework.Oid4Vp.Models;

namespace WalletFramework.Oid4Vp.RelyingPartyAuthentication.Abstractions;

public interface IRpAuthService
{
    public Task<RpAuthResult> Authenticate(RequestObject requestObject);
}
