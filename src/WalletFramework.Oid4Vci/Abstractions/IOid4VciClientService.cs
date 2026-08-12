using LanguageExt;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Localization;
using WalletFramework.Credentials.CredentialSet.Models;
using WalletFramework.Oid4Vci.AuthFlow.Models;
using WalletFramework.Oid4Vci.CredOffer.Models;

namespace WalletFramework.Oid4Vci.Abstractions;

/// <summary>
///     Provides an interface for services related to OpenID for Verifiable Credential Issuance.
/// </summary>
public interface IOid4VciClientService
{
    /// <summary>
    ///     Requests a verifiable credential using the pre-authorized code flow.
    /// </summary>
    /// ///
    /// <param name="credentialOfferMetadata">Credential offer and Issuer Metadata</param>
    /// <param name="transactionCode">The Transaction Code.</param>
    Task<Validation<IEnumerable<CredentialDataSet>>> AcceptOffer(
        CredentialOfferMetadata credentialOfferMetadata,
        string? transactionCode);

    /// <summary>
    ///     Initiates the issuer initiated authorization process of the VCI authorization code flow.
    /// </summary>
    /// <param name="offer">The offer metadata</param>
    /// <returns></returns>
    Task<Uri> InitiateAuthFlow(CredentialOfferMetadata offer);

    /// <summary>
    ///     Processes a credential offer
    /// </summary>
    /// <param name="credentialOffer">The credential offer uri</param>
    /// <param name="language">Optional language tag</param>
    Task<Validation<CredentialOfferMetadata>> ProcessOffer(
        Uri credentialOffer,
        Option<Locale> language);

    /// <summary>
    ///     Requests a verifiable credential using the authorization code flow.
    /// </summary>
    /// <param name="issuanceSession">Holds authorization session relevant information.</param>
    /// <returns>
    ///     A list of credentials.
    /// </returns>
    Task<Validation<IEnumerable<CredentialDataSet>>> RequestCredentialSet(IssuanceSession issuanceSession);
}
