#nullable enable

using System;
using System.Threading.Tasks;
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
        /// <param name="preAuthorizedCode">The pre-authorized code for token request.</param>
        /// <param name="transactionCode">The Transaction Code.</param>
        /// <param name="userPin">The User Pin.</param>
        /// <returns>
        ///     A tuple containing the credential response and the key ID used during the signing of the Proof of Possession.
        /// </returns>
        Task<(OidCredentialResponse credentialResponse, string keyId)> RequestCredentialAsync(
            OidCredentialMetadata credentialMetadata,
            OidIssuerMetadata issuerMetadata,
            string preAuthorizedCode,
            string? transactionCode,
            string? userPin
        );
    }
}
