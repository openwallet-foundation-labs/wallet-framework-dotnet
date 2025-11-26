using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

/// <inheritdoc />
internal class Oid4VpHaipClient(IDcqlService dcqlService) : IOid4VpHaipClient
{
    /// <inheritdoc />
    public Task<AuthorizationResponse> CreateAuthorizationResponseAsync(
        AuthorizationRequest authorizationRequest,
        PresentationMap[] presentationMaps)
    {
        var result = dcqlService.CreateAuthorizationResponse(authorizationRequest, presentationMaps);
        return Task.FromResult(result);
    }
}
