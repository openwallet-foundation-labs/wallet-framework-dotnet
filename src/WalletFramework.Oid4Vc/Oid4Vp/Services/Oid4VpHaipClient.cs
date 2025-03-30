using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;
using static Newtonsoft.Json.JsonConvert;
using Format = WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Format;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

/// <inheritdoc />
internal class Oid4VpHaipClient(
    IPexService pexService,
    IDcqlService dcqlService) : IOid4VpHaipClient
{
    /// <inheritdoc />
    public Task<AuthorizationResponse> CreateAuthorizationResponseAsync(
        AuthorizationRequest authorizationRequest,
        (string Identifier, string Presentation, Format Format)[] presentationMap)
    {
        return authorizationRequest.Requirements.Match(
            dcqlQuery => Task.FromResult(dcqlService.CreateAuthorizationResponse(authorizationRequest, presentationMap)),
            presentationDefinition => pexService.CreateAuthorizationResponseAsync(authorizationRequest, presentationMap));
    }
}
