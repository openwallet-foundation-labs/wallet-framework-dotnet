namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization
{
    /// <summary>
    ///     Identifier of the authorization session during the VCI Authorization Code Flow.
    /// </summary>
    public struct State
    {
        /// <summary>
        ///    Gets the value of the session identifier.
        /// </summary>
        private string Value { get; }
        
        private State(string value) => Value = value;
        
        /// <summary>
        ///     Returns the value of the session identifier.
        /// </summary>
        /// <param name="sessionParameters"></param>
        /// <returns></returns>
        public static implicit operator string(State sessionParameters) => sessionParameters.Value;
        
        public static State CreateState(string state)
        {
            if (!Guid.TryParse(state, out _))
            {
                throw new ArgumentException("SessionId must not be a Guid");
            }
            
            return new State(state);
        }
    }
}
