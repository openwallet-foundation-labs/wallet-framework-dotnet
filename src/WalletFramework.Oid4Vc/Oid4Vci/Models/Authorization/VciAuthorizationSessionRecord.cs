using Hyperledger.Aries.Storage;
using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vci.Models.CredentialOffer.GrantTypes;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization
{
    /// <summary>
    ///   Represents the authorization session record. Used during the VCI Authorization Code Flow to hold session relevant information.
    /// </summary>
    public sealed class VciAuthorizationSessionRecord : RecordBase
    {
        /// <summary>
        ///     The AuthroizationCode from the CredentialOffer associated with the session.
        /// </summary>
        public AuthorizationCode AuthorizationCode { get; }
        
        /// <summary>
        ///     The parameters for the 'authorization_code' grant type.
        /// </summary>
        public AuthorizationCodeParameters AuthorizationCodeParameters { get; }
        
        /// <summary>
        ///     The client options that are used during the VCI Authorization Code Flow. Here the wallet acts as the client.
        /// </summary>
        public ClientOptions ClientOptions { get; }
        
        /// <summary>
        ///     The metadata set that is associated with the session.
        /// </summary>
        public MetadataSet MetadataSet { get; }
        
        /// <summary>
        ///     The credential configuration identifiers that are associated with the session. They represent the credentials
        ///     the wallet wants to request.
        /// </summary>
        public string[] CredentialConfigurationIds { get; }

        /// <summary>
        ///    Initializes a new instance of the <see cref="VciAuthorizationSessionRecord" /> class.
        /// </summary>
        public override string TypeName  => "AF.VciAuthorizationSessionRecord";
        
#pragma warning disable CS8618
        /// <summary>
        ///   Initializes a new instance of the <see cref="VciAuthorizationSessionRecord" /> class.
        /// </summary>
        public VciAuthorizationSessionRecord()
        {
        }
#pragma warning restore CS8618
    
        /// <summary>
        ///   Initializes a new instance of the <see cref="VciAuthorizationSessionRecord" /> class.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="authorizationCodeParameters"></param>
        /// <param name="authorizationCode"></param>
        /// <param name="clientOptions"></param>
        /// <param name="metadataSet"></param>
        /// <param name="credentialConfigurationIds"></param>
        [JsonConstructor]
        public VciAuthorizationSessionRecord(
            VciSessionId sessionId,
            AuthorizationCodeParameters authorizationCodeParameters,
            AuthorizationCode authorizationCode,
            ClientOptions clientOptions,
            MetadataSet metadataSet,
            string[] credentialConfigurationIds)
        {
            Id = sessionId;
            RecordVersion = 1;
            AuthorizationCodeParameters = authorizationCodeParameters;
            AuthorizationCode = authorizationCode;
            ClientOptions = clientOptions;
            MetadataSet = metadataSet;
            CredentialConfigurationIds = credentialConfigurationIds;
        }
    }
}
