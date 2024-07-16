using WalletFramework.Core.Functional;
using static System.Web.HttpUtility;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

/// <summary>
///   Represents the parameters of an VCI Authorization Code Flow issuance session.
/// </summary>
public record IssuanceSession
{
    /// <summary>
    ///   Gets the session identifier.
    /// </summary>
    public VciSessionId SessionId { get; }
        
    /// <summary>
    ///  Gets the actual authorization code that is received from the authorization server upon successful authorization.
    /// </summary>
    public string Code { get; }
        
    private IssuanceSession(VciSessionId sessionId, string code) => (SessionId, Code) = (sessionId, code);
        
    /// <summary>
    ///    Creates a new instance of <see cref="IssuanceSession"/> from the given <see cref="Uri"/>.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IssuanceSession FromUri(Uri uri)
    {
        var queryParams = ParseQueryString(uri.Query);
        
        var code = queryParams.Get("code");
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new InvalidOperationException("Query parameter 'code' is missing");
        }
        
        var sessionIdParam = queryParams.Get("session");
        var sessionId = VciSessionId.ValidSessionId(sessionIdParam).Fallback(VciSessionId.CreateSessionId());

        return new IssuanceSession(sessionId, code);
    }
}
