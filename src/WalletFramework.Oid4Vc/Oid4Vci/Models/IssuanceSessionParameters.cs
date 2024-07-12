using WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization;
using static System.Web.HttpUtility;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models
{
    /// <summary>
    ///   Represents the parameters of an VCI Authorization Code Flow issuance session.
    /// </summary>
    public record IssuanceSessionParameters
    {
        /// <summary>
        ///   Gets the session identifier.
        /// </summary>
        public State State { get; }
        
        /// <summary>
        ///  Gets the actual authorization code that is received from the authorization server upon succesful authorization.
        /// </summary>
        public string Code { get; }
        
        private IssuanceSessionParameters(State sessionId, string code) => (State, Code) = (sessionId, code);
        
        /// <summary>
        ///    Creates a new instance of <see cref="IssuanceSessionParameters"/> from the given <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IssuanceSessionParameters FromUri(Uri uri)
        {
            var queryParams = ParseQueryString(uri.Query);
        
            var code = queryParams.Get("code");
            var state = queryParams.Get("state");
        
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
            {
                throw new InvalidOperationException("Query parameter 'code' and/or 'state' are missing");
            }

            return new IssuanceSessionParameters(State.CreateState(state), code);
        }
    }
}
