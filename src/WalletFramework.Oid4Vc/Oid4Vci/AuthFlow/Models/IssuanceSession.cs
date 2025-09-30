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
    public AuthFlowSessionCode AuthFlowSessionCode { get; }
        
    private IssuanceSession(AuthFlowSessionState authFlowSessionState, AuthFlowSessionCode authFlowSessionCode) => 
        (AuthFlowSessionState, AuthFlowSessionCode) = (authFlowSessionState, authFlowSessionCode);
        
    /// <summary>
    ///    Creates a new instance of <see cref="IssuanceSession"/> from the given <see cref="Uri"/>.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Validation<IssuanceSession> FromUri(Uri uri)
    {
        var queryParams = ParseQueryString(uri.Query);
        
        var code = queryParams.Get("code");
        var authFlowSessionCodeValidation = AuthFlowSessionCode.ValidAuthFlowSessionCode(code);
        
        var sessionStateParam = queryParams.Get("state");
        var authFlowSessionStateValidation = AuthFlowSessionState.ValidAuthFlowSessionState(sessionStateParam);

        return from authFlowSessionCode in authFlowSessionCodeValidation 
            from authFlowSessionState in authFlowSessionStateValidation
            select new IssuanceSession(authFlowSessionState, authFlowSessionCode);
    }
}
