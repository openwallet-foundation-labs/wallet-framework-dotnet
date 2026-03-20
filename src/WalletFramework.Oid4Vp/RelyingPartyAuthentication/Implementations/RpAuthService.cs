using WalletFramework.Oid4Vp.Models;
using WalletFramework.Oid4Vp.RelyingPartyAuthentication.Abstractions;

namespace WalletFramework.Oid4Vp.RelyingPartyAuthentication.Implementations;

public class RpAuthService(IRpRegistrarService irpRegistrarService) : IRpAuthService
{
    public async Task<RpAuthResult> Authenticate(RequestObject requestObject)
    {
        var rpRegistrarCertificate = await irpRegistrarService.FetchRpRegistrarCertificate();
        return RpAuthResult.ValidateRequestObject(requestObject, rpRegistrarCertificate);
    }
}
