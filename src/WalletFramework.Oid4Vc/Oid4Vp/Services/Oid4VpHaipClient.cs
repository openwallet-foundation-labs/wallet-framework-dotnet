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
    public Task<AuthorizationResponse> CreateAuthorizationResponseAsync(
        AuthorizationRequest authorizationRequest,
        PresentationMap[] presentationMaps)
    {
        return authorizationRequest.Requirements.Match(
            dcqlQuery => Task.FromResult(dcqlService.CreateAuthorizationResponse(authorizationRequest, presentationMaps)),
            presentationDefinition => pexService.CreateAuthorizationResponseAsync(authorizationRequest, presentationMaps));
    }
}
