using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

/// <inheritdoc />
internal class Oid4VpHaipClient(
    IPexService pexService,
    IDcqlService dcqlService) : IOid4VpHaipClient
{
    /// <inheritdoc />
    public async Task<AuthorizationResponse> CreateAuthorizationResponseAsync(
        AuthorizationRequest authorizationRequest,
        PresentationMap[] presentationMaps)
    {
        return await authorizationRequest.Requirements.Match(
            _ =>
            {
                var result = dcqlService.CreateAuthorizationResponse(authorizationRequest, presentationMaps);
                return Task.FromResult(result);
            },
            async _ =>
            {
                return await pexService.CreateAuthorizationResponseAsync(authorizationRequest, presentationMaps);
            }
        );
    }
}
