namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization
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
        
        public static VciSessionId CreateSessionId(string sessionId)
        {
            if (!Guid.TryParse(sessionId, out _))
            {
                throw new ArgumentException("SessionId must not be a Guid");
            }
            
            return new VciSessionId(sessionId);
        }
    }
}
