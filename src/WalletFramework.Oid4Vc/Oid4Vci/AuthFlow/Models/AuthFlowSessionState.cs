using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

/// <summary>
///     Identifier of the authorization state during the VCI Authorization Code Flow.
/// </summary>
public struct AuthFlowSessionState
{
    /// <summary>
    ///    Gets the value of the state identifier.
    /// </summary>
    private string Value { get; }
        
    private AuthFlowSessionState(string value) => Value = value;
        
    /// <summary>
    ///     Returns the value of the state identifier.
    /// </summary>
    /// <param name="authFlowSessionState"></param>
    public static implicit operator string(AuthFlowSessionState authFlowSessionState) => authFlowSessionState.Value;
        
    public static Validation<AuthFlowSessionState> ValidAuthFlowSessionState(string authFlowSessionState)
    {
        if (!Guid.TryParse(authFlowSessionState, out _))
        {
            return new AuthFlowSessionStateError(authFlowSessionState);
        }
            
        return new AuthFlowSessionState(authFlowSessionState);
    }
    
    public static AuthFlowSessionState CreateAuthFlowSessionState()
    {
        var guid = Guid.NewGuid().ToString();
        return new AuthFlowSessionState(guid);
    }
}

public static class AuthFlowSessionStateFun
{
    public static AuthFlowSessionState DecodeFromJson(JValue json) => AuthFlowSessionState
        .ValidAuthFlowSessionState(json.ToString(CultureInfo.InvariantCulture))
        .UnwrapOrThrow(new InvalidOperationException("AuthFlowSessionState is corrupt"));
}
