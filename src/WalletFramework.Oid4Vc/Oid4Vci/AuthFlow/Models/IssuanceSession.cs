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
    public AuthFlowSessionState AuthFlowSessionState { get; }
        
    /// <summary>
    ///  Gets the actual authorization code that is received from the authorization server upon successful authorization.
    /// </summary>
    public string Code { get; }
        
    private IssuanceSession(AuthFlowSessionState authFlowSessionState, string code) => (AuthFlowSessionState, Code) = (authFlowSessionState, code);
        
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
        
        var sessionStateParam = queryParams.Get("state");
        var authFlowSessionState = AuthFlowSessionState.ValidAuthFlowSessionState(sessionStateParam).Fallback(AuthFlowSessionState.CreateAuthFlowSessionState());

        return new IssuanceSession(authFlowSessionState, code);
    }
}
