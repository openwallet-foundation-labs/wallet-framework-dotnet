using WalletFramework.Oid4Vp.Dcql.Services;
using WalletFramework.Oid4Vp.Models;

namespace WalletFramework.Oid4Vp.Services;

/// <inheritdoc />
public class Oid4VpHaipClient(IDcqlService dcqlService) : IOid4VpHaipClient
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
