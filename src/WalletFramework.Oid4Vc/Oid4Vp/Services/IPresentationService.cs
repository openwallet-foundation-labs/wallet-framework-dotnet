using LanguageExt;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

/// <summary>
///     Service responsible for creating presentation maps from selected credentials.
/// </summary>
public interface IPresentationService
{
    /// <summary>
    ///     Creates presentation maps from the provided authorization request and selected credentials.
    /// </summary>
    /// <param name="authorizationRequest">The authorization request containing requirements.</param>
    /// <param name="selectedCredentials">The credentials selected for presentation.</param>
    /// <param name="origin">The origin of the request.</param>
    /// <returns>A task that returns a list of presentation maps with their corresponding presented credentials and optional mdoc nonce.</returns>
    Task<(List<(PresentationMap PresentationMap, ICredential PresentedCredential)> Presentations, Option<Nonce> MdocNonce)> CreatePresentations(
        AuthorizationRequest authorizationRequest,
        IEnumerable<SelectedCredential> selectedCredentials,
        Option<Origin> origin);
    
    Task<(List<(PresentationMap PresentationMap, ICredential PresentedCredential)> Presentations, Option<Nonce> MdocNonce)> CreatePresentations(
        AuthorizationRequest authorizationRequest,
        IEnumerable<SelectedCredential> selectedCredentials) => 
        CreatePresentations(authorizationRequest, selectedCredentials.ToList(), Option<Origin>.None);
} 
