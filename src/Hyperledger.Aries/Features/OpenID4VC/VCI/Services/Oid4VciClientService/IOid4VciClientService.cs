#nullable enable

using System;
using System.Threading.Tasks;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Authorization;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialResponse;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Credential;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Issuer;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vci.Services.Oid4VciClientService
{
    /// <summary>
    ///     Provides an interface for services related to OpenID for Verifiable Credential Issuance.
    /// </summary>
    public interface IOid4VciClientService
    {
        /// <summary>
        ///     Fetches the metadata related to the OID issuer from the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint URL to retrieve the issuer metadata.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the OID issuer metadata.</returns>
        Task<OidIssuerMetadata> FetchIssuerMetadataAsync(Uri endpoint);

        /// <summary>
        ///     Requests a verifiable credential using the provided parameters.
        /// </summary>
        /// <param name="credentialMetadata">The credential metadata.</param>
        /// <param name="issuerMetadata">The issuer metadata.</param>
        /// <param name="tokenResponse">The token response from the previous token request.</param>
        /// <param name="dPopKeyId">The key ID that is used during the signing of the DPoP Jwt Proof.</param>
        /// <param name="dPopNonce">The nonce that is used for the DPoP Jwt Proof.</param>
        /// <returns>
        ///     A tuple containing the credential response and the key ID used during the signing of the Proof of Possession.
        /// </returns>
        Task<(OidCredentialResponse credentialResponse, string keyId)> RequestCredentialAsync(
            OidCredentialMetadata credentialMetadata,
            OidIssuerMetadata issuerMetadata,
            TokenResponse tokenResponse,
            string? dPopKeyId,
            string? dPopNonce
        );

        /// <summary>
        ///     Requests a token using the provided issuer metadata and pre-authorized code.
        /// </summary>
        /// <param name="metadata">The OID issuer metadata.</param>
        /// <param name="preAuthorizedCode">The pre-authorized code for token request.</param>
        /// <param name="transactionCode">The Transaction Code.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the token response
        /// and a key ID and nonce for the DPoP process.</returns>
        Task<(TokenResponse tokenResponse, string? dPopKeyId, string? dPopNonce)> RequestTokenAsync(
            OidIssuerMetadata metadata,
            string preAuthorizedCode,
            string? transactionCode = null
        );
    }
}
