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
        public VciSessionId SessionId { get; }
        
        /// <summary>
        ///  Gets the actual authorization code that is received from the authorization server upon succesful authorization.
        /// </summary>
        public string Code { get; }
        
        private IssuanceSessionParameters(VciSessionId sessionId, string code) => (SessionId, Code) = (sessionId, code);
        
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
            var sessionId = queryParams.Get("session");
        
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(sessionId))
            {
                throw new InvalidOperationException("Query parameter 'code' and/or 'session' are missing");
            }

            return new IssuanceSessionParameters(sessionId, code);
        }
    }
}
