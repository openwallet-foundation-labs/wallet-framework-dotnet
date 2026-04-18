using OneOf;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

/// <inheritdoc />
public class DcApiService : IDcApiService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DcApiService" /> class.
    /// </summary>
    /// <param name="dcqlService">The DCQL service.</param>
    /// <param name="presentationService">The presentation service.</param>
    /// <param name="oid4VpHaipClient">The OID4VP HAIP client.</param>
    /// <param name="authorizationResponseEncryptionService">The authorization response encryption service.</param>
    public DcApiService(
        IDcqlService dcqlService,
        IPresentationService presentationService,
        IOid4VpHaipClient oid4VpHaipClient,
        IAuthorizationResponseEncryptionService authorizationResponseEncryptionService)
    {
        _dcqlService = dcqlService;
        _presentationService = presentationService;
        _oid4VpHaipClient = oid4VpHaipClient;
        _authorizationResponseEncryptionService = authorizationResponseEncryptionService;
    }

    private readonly IDcqlService _dcqlService;
    private readonly IPresentationService _presentationService;
    private readonly IOid4VpHaipClient _oid4VpHaipClient;
    private readonly IAuthorizationResponseEncryptionService _authorizationResponseEncryptionService;

    /// <inheritdoc />
    public async Task<PresentationRequest> ProcessDcApiRequest(AuthorizationRequest dcApiRequest)
    {
        var candidateQueryResult = await _dcqlService.Query(dcApiRequest.DcqlQuery);
        var presentationRequest = new PresentationRequest(dcApiRequest, candidateQueryResult);
        
        var vpTxDataOption = dcApiRequest.TransactionData;
        return vpTxDataOption.Match(
            vpTxData => TransactionDataFun.ProcessVpTransactionData(presentationRequest, vpTxData).UnwrapOrThrow(),
            () => presentationRequest
        );
    }
    

    /// <inheritdoc />
    public async Task<OneOf<AuthorizationResponse, EncryptedAuthorizationResponse>> AcceptDcApiRequest(
        DcApiRequestItem dcApiRequestItem,
        IEnumerable<SelectedCredential> selectedCredentials)
    {
        var authorizationRequest = dcApiRequestItem.Data;
        var origin = dcApiRequestItem.Origin;
        
        var (presentations, mdocNonce) = await _presentationService.CreatePresentations(
            authorizationRequest, 
            selectedCredentials,
            origin);

        var authorizationResponse = await _oid4VpHaipClient.CreateAuthorizationResponseAsync(
            authorizationRequest,
            presentations.Select(tuple => tuple.PresentationMap).ToArray()
        );

        return authorizationRequest.ResponseMode switch
        {
            AuthorizationRequest.DcApi => authorizationResponse,
            AuthorizationRequest.DcApiJwt => await _authorizationResponseEncryptionService.Encrypt(
                authorizationResponse, authorizationRequest, mdocNonce),
            _ => throw new InvalidOperationException($"{nameof(authorizationRequest.ResponseMode)} is not valid at this point")
        };
    }
} 
