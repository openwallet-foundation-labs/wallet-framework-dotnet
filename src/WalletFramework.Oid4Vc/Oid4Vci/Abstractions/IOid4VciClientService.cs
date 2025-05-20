using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;
using OneOf;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Localization;
using WalletFramework.MdocLib;
using WalletFramework.Oid4Vc.CredentialSet.Models;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.SdJwtVc.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.Abstractions;

/// <summary>
///     Provides an interface for services related to OpenID for Verifiable Credential Issuance.
/// </summary>
public interface IOid4VciClientService
{
    /// <summary>
    ///    Initiates the issuer initiated authorization process of the VCI authorization code flow.
    /// </summary>
    /// <param name="offer">The offer metadata</param>
    /// <param name="clientOptions">The client options</param>
    /// <returns></returns>
    Task<Uri> InitiateAuthFlow(CredentialOfferMetadata offer, ClientOptions clientOptions);
    
    /// <summary>
    ///    Initiates the wallet initiate authorization process of the VCI authorization code flow.
    /// </summary>
    /// <param name="uri">The issuers uri</param>
    /// <param name="clientOptions">The client options</param>
    /// <param name="language">Optional language tag</param>
    /// <param name="credentialType">Specifies whether Sd-Jwt or MDoc should be issued</param>
    /// <param name="specVersion">Optional language tag</param>
    /// <returns></returns>
    Task<Uri> InitiateAuthFlow(Uri uri, ClientOptions clientOptions, Option<Locale> language, Option<OneOf<Vct, DocType>> credentialType, int specVersion);
        
    /// <summary>
    ///     Requests a verifiable credential using the authorization code flow.
    /// </summary>
    /// <param name="issuanceSession">Holds authorization session relevant information.</param>
    /// <returns>
    /// A list of credentials.
    /// </returns>
    Task<Validation<IEnumerable<CredentialSetRecord>>> RequestCredentialSet(IssuanceSession issuanceSession);
    
    /// <summary>
    ///     Requests a verifiable credential using the authorization code flow and C''.
    /// </summary>
    /// <param name="issuanceSession">Holds authorization session relevant information.</param>
    /// <param name="authorizationRequest">The AuthorizationRequest that is associated witht the ad-hoc crednetial issuance</param>
    /// <returns>
    /// A list of credentials.
    /// </returns>
    Task<Validation<IEnumerable<OnDemandCredentialSet>>> RequestOnDemandCredentialSet(IssuanceSession issuanceSession, AuthorizationRequest authorizationRequest);
    
    /// <summary>
    ///     Processes a credential offer
    /// </summary>
    /// <param name="credentialOffer">The credential offer uri</param>
    /// <param name="language">Optional language tag</param>
    Task<Validation<CredentialOfferMetadata>> ProcessOffer(Uri credentialOffer, Option<Locale> language);

    /// <summary>
    ///     Requests a verifiable credential using the pre-authorized code flow.
    /// </summary>
    /// ///
    /// <param name="credentialOfferMetadata">Credential offer and Issuer Metadata</param>
    /// <param name="transactionCode">The Transaction Code.</param>
    Task<Validation<IEnumerable<CredentialSetRecord>>> AcceptOffer(
        CredentialOfferMetadata credentialOfferMetadata,
        string? transactionCode);
}
