using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.Abstractions;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.Implementations;

public class RpAuthService(IRpRegistrarService irpRegistrarService) : IRpAuthService
{
    public async Task<RpAuthResult> Authenticate(RequestObject requestObject)
    {
        var rpRegistrarCertificate = await irpRegistrarService.FetchRpRegistrarCertificate();
        return RpAuthResult.ValidateRequestObject(requestObject, rpRegistrarCertificate);
    }
}
