using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

/// <summary>
///     Identifier of the authorization code during the VCI Authorization Code Flow that can be used to retrieve an access token.
/// </summary>
public struct AuthFlowSessionCode
{
    /// <summary>
    ///    Gets the value of the code associated with an issuance session.
    /// </summary>
    private string Value { get; }
        
    private AuthFlowSessionCode(string value) => Value = value;
        
    /// <summary>
    ///     Returns the value of the code associated with an issuance session.
    /// </summary>
    /// <param name="authFlowSessionCode"></param>
    /// <returns></returns>
    public static implicit operator string(AuthFlowSessionCode authFlowSessionCode) => authFlowSessionCode.Value;
        
    public static Validation<AuthFlowSessionCode> ValidAuthFlowSessionCode(string authFlowSessionCode)
    {
        if (string.IsNullOrWhiteSpace(authFlowSessionCode))
        {
            return new AuthFlowSessionCodeError(authFlowSessionCode);
        }
            
        return new AuthFlowSessionCode(authFlowSessionCode);
    }
}
