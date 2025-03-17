using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;
using static Newtonsoft.Json.JsonConvert;
using Format = WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Format;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

/// <inheritdoc />
internal class Oid4VpHaipClient : IOid4VpHaipClient
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Oid4VpHaipClient" /> class.
    /// </summary>
    /// <param name="pexService">The service responsible for presentation exchange protocol operations.</param>
    /// <param name="dcqlService">The service responsibl for DCQL operations.</param>
    public Oid4VpHaipClient(IPexService pexService,
        IDcqlService dcqlService)
    {
        _pexService = pexService;
        _dcqlService = dcqlService;
    }

    private readonly IPexService _pexService;
    private readonly IDcqlService _dcqlService;

    /// <inheritdoc />
    public Task<AuthorizationResponse> CreateAuthorizationResponseAsync(
        AuthorizationRequest authorizationRequest,
        (string Identifier, string Presentation, Format Format)[] presentationMap)
    {
        return authorizationRequest.IsPexRequest()
            ? _pexService.CreateAuthorizationResponseAsync(authorizationRequest, presentationMap)
            : Task.FromResult(_dcqlService.CreateAuthorizationResponse(authorizationRequest, presentationMap));
    }
}
