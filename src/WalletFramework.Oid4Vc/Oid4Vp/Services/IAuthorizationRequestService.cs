using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

public interface IAuthorizationRequestService
{
    Task<AuthorizationRequest> CreateAuthorizationRequest(AuthorizationRequestByReference authorizationRequestUri);
    
    Task<AuthorizationRequest> CreateAuthorizationRequest(AuthorizationRequestByValue authorizationRequestUri);
}
