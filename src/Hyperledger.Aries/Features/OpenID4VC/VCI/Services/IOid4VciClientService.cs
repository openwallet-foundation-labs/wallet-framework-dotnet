#nullable enable

using System;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.OpenID4VC.VCI.Models;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Authorization;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialOffer.GrantTypes;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialResponse;
using Hyperledger.Aries.Features.OpenID4VC.VCI.Models.Metadata;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Credential;

namespace Hyperledger.Aries.Features.OpenID4VC.VCI.Services
{
    /// <summary>
    ///     Provides an interface for services related to OpenID for Verifiable Credential Issuance.
    /// </summary>
    public interface IOid4VciClientService
    {
        /// <summary>
        ///     Fetches the metadata related to the OID issuer from the specified endpoint.
        /// </summary>
        /// <param name="issuerEndpoint">The endpoint URL to retrieve the issuer metadata.</param>
        /// <param name="preferredLanguage">The preferred language of the wallet in which it would like to retrieve the issuer metadata. The default is "en"</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the OID issuer metadata.</returns>
        //Task<OidIssuerMetadata> FetchIssuerMetadataAsync(Uri endpoint, string preferredLanguage = "en");
        Task<MetadataSet> FetchMetadataAsync(Uri issuerEndpoint, string preferredLanguage = "en");

        /// <summary>
        ///    Initiates the authorization process of the VCI authorization code flow.
        /// </summary>
        /// <param name="agentContext">Agent Context</param>
        /// <param name="authorizationCode"></param>
        /// <param name="clientOptions"></param>
        /// <param name="metadataSet"></param>
        /// <param name="credentialConfigurationIds"></param>
        /// <returns></returns>
        Task<Uri> InitiateAuthentication(
            IAgentContext agentContext,
            AuthorizationCode authorizationCode,
            ClientOptions clientOptions,
            MetadataSet metadataSet,
            string[] credentialConfigurationIds);

        /// <summary>
        ///     Requests a verifiable credential using the pre authorized code flow.
        /// </summary>
        /// <param name="metadataSet">Holds the Issuer Metadata and Authorization Server Metadata</param>
        /// <param name="credentialMetadata">The credential metadata.</param>
        /// <param name="preAuthorizedCode">The pre-authorized code for token request.</param>
        /// /// <param name="transactionCode">The Transaction Code.</param>
        /// <returns>
        ///     A tuple containing the credential response and the key ID used during the signing of the Proof of Possession.
        /// </returns>
        Task<(OidCredentialResponse credentialResponse, string keyId)[]> RequestCredentialAsync(
            MetadataSet metadataSet,
            OidCredentialMetadata credentialMetadata,
            string preAuthorizedCode,
            string? transactionCode
        );
        
        /// <summary>
        ///     Requests a verifiable credential using the authorization code flow.
        /// </summary>
        /// <param name="context">The agent context.</param>
        /// <param name="issuanceSessionParameters">Holds authorization session relevant information.</param>
        /// <returns>
        ///     A tuple containing the credential response and the key ID used during the signing of the Proof of Possession.
        /// </returns>
        Task<(OidCredentialResponse credentialResponse, string keyId)[]> RequestCredentialAsync(
            IAgentContext context,
            IssuanceSessionParameters issuanceSessionParameters
        );
    }
}
