using LanguageExt;
using WalletFramework.Oid4Vp.Models;

namespace WalletFramework.Oid4Vp.Services;

public interface IAuthorizationRequestService
{
    Task<Validation<AuthorizationRequestCancellation, AuthorizationRequest>> GetAuthorizationRequest(
        AuthorizationRequestUri authorizationRequestUri);
}
