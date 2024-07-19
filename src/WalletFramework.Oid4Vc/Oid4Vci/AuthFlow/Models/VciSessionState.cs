using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

/// <summary>
///     Identifier of the authorization state during the VCI Authorization Code Flow.
/// </summary>
public struct VciSessionState
{
    /// <summary>
    ///    Gets the value of the state identifier.
    /// </summary>
    private string Value { get; }
        
    private VciSessionState(string value) => Value = value;
        
    /// <summary>
    ///     Returns the value of the state identifier.
    /// </summary>
    /// <param name="vciSessionState"></param>
    /// <returns></returns>
    public static implicit operator string(VciSessionState vciSessionState) => vciSessionState.Value;
        
    public static Validation<VciSessionState> ValidVciSessionState(string vciSessionState)
    {
        if (!Guid.TryParse(vciSessionState, out _))
        {
            return new VciSessionStateError(vciSessionState);
        }
            
        return new VciSessionState(vciSessionState);
    }
    
    public static VciSessionState CreateVciSessionState()
    {
        var guid = Guid.NewGuid().ToString();
        return new VciSessionState(guid);
    }
}

public static class VciSessionStateFun
{
    public static VciSessionState DecodeFromJson(JValue json) => VciSessionState
        .ValidVciSessionState(json.ToString(CultureInfo.InvariantCulture))
        .UnwrapOrThrow(new InvalidOperationException("VciSessionState is corrupt"));
}
