using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

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
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public static implicit operator string(VciSessionId sessionId) => sessionId.Value;
        
    public static Validation<VciSessionId> ValidSessionId(string sessionId)
    {
        if (!Guid.TryParse(sessionId, out _))
        {
            return new VciSessionIdError(sessionId);
        }
            
        return new VciSessionId(sessionId);
    }
    
    public static VciSessionId CreateSessionId()
    {
        var guid = Guid.NewGuid().ToString();
        return new VciSessionId(guid);
    }
}

public static class VciSessionIdFun
{
    public static VciSessionId DecodeFromJson(JValue json) => VciSessionId
        .ValidSessionId(json.ToString(CultureInfo.InvariantCulture))
        .UnwrapOrThrow(new InvalidOperationException("SessionId is corrupt"));
}
