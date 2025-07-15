using LanguageExt;
using OneOf;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption;
using WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

/// <summary>
///     Service responsible for handling DC API operations.
/// </summary>
public interface IDcApiService
{
    /// <summary>
    ///     Processes a DC API request and returns presentation candidates.
    /// </summary>
    /// <param name="dcApiRequest">The DC API request to process.</param>
    /// <returns>A task that returns the presentation request with candidates.</returns>
    Task<PresentationRequest> ProcessDcApiRequest(AuthorizationRequest dcApiRequest);
    
    /// <summary>
    ///     Accepts a DC API request and creates an authorization response.
    /// </summary>
    /// <param name="dcApiRequestItem">The DC API request item to accept.</param>
    /// <param name="selectedCredentials">The credentials selected for presentation.</param>
    /// <returns>A task that returns either a plain or encrypted authorization response.</returns>
    Task<OneOf<AuthorizationResponse, EncryptedAuthorizationResponse>> AcceptDcApiRequest(
        DcApiRequestItem dcApiRequestItem,
        IEnumerable<SelectedCredential> selectedCredentials);
} 
