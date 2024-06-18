using Hyperledger.Aries.Storage;
using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization
{
    /// <summary>
    ///   Represents the authorization session record. Used during the VCI Authorization Code Flow to hold session relevant information.
    /// </summary>
    public sealed class VciAuthorizationSessionRecord : RecordBase
    {
        /// <summary>
        ///     The session specific id.
        /// </summary>
        [JsonIgnore]
        public VciSessionId SessionId
        {
            get => VciSessionId.CreateSessionId(Get());
            set => Set((string)value, false);
        }

        /// <summary>
        ///     The Authorization Code from the CredentialOffer associated with the session. Only needed within the Pre Authorization Code flow.
        /// </summary>
        public AuthorizationData AuthorizationData { get; }
        
        /// <summary>
        ///     The parameters for the 'authorization_code' grant type.
        /// </summary>
        public AuthorizationCodeParameters AuthorizationCodeParameters { get; }

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
        /// <param name="authorizationData"></param>
        /// <param name="authorizationCodeParameters"></param>
        [JsonConstructor]
        public VciAuthorizationSessionRecord(
            VciSessionId sessionId,
            AuthorizationData authorizationData,
            AuthorizationCodeParameters authorizationCodeParameters)
        {
            SessionId = sessionId;
            Id = Guid.NewGuid().ToString();
            RecordVersion = 1;
            AuthorizationCodeParameters = authorizationCodeParameters;
            AuthorizationData = authorizationData;
        }
    }
}
