using Hyperledger.Aries.Agents;
using WalletFramework.Oid4Vc.Oid4Vci.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization;
using WalletFramework.Oid4Vc.Oid4Vci.Models.CredentialOffer.GrantTypes;
using WalletFramework.Oid4Vc.Oid4Vci.Models.CredentialResponse;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Credential;

namespace WalletFramework.Oid4Vc.Oid4Vci.Services.Oid4VciClientService
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
        /// <param name="clientOptions">Options specified by the Client (Wallet)</param>
        /// <param name="metadataSet">Consists of Issuer and Credential Metadata</param>
        /// <param name="credentialConfigurationIds">Identifiers of the Credentials that will be requested</param>
        /// <param name="authorizationCode">Authorization Code from the Credential Offer. Only used within the Issuer Initiated Authorization Code Flow</param>
        /// <returns></returns>
        Task<Uri> InitiateAuthentication(
            IAgentContext agentContext,
            ClientOptions clientOptions,
            MetadataSet metadataSet,
            string[] credentialConfigurationIds,
            AuthorizationCode? authorizationCode);

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
