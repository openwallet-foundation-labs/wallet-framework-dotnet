namespace Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Authorization
{
    /// <summary>
    ///     Identifier of the authorization session during the VCI Authorization Code Flow.
    /// </summary>
    public struct VciSessionId
    {
        /// <summary>
        ///    Gets the value of the session identifier.
        /// </summary>
        private string Value { get; }
        
        private VciSessionId(string value) => Value = value;
        
        /// <summary>
        ///     Returns the value of the session identifier.
        /// </summary>
        /// <param name="sessionParameters"></param>
        /// <returns></returns>
        public static implicit operator string(VciSessionId sessionParameters) => sessionParameters.Value;
        
        /// <summary>
        ///     Creates a new instance of <see cref="VciSessionId"/> from the given <see cref="string"/>.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static implicit operator VciSessionId(string sessionId) => CreateSessionId(sessionId);
        
        private static VciSessionId CreateSessionId(string sessionId) => new (sessionId);
    }
}
