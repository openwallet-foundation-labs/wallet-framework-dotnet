using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

/// <summary>
///  Represents an OpenID4VP Authorization Response.
/// </summary>
public class AuthorizationResponse
{
    /// <summary>
    ///     Gets or sets the VP Token.
    /// </summary>
    [JsonProperty("vp_token")]
    [JsonConverter(typeof(VpTokenJsonConverter))]
    public VpToken VpToken { get; set; } = null!;
        
    /// <summary>
    ///     Gets or sets the State.
    /// </summary>
    [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
    public string? State { get; set; }
}

public static class AuthorizationResponseFun
{
    public static FormUrlEncodedContent ToFormUrl(this AuthorizationResponse authorizationResponse)
    {
        var dict = new Dictionary<string, string>
        {
            { "vp_token", authorizationResponse.VpToken.AsJsonString() },
            { "state", authorizationResponse.State ?? string.Empty }
        };

        return new FormUrlEncodedContent(dict);
    }

    public static JObject AsJObject(this AuthorizationResponse authorizationResponse)
    {
        var jObject = new JObject
        {
            { "vp_token", authorizationResponse.VpToken.AsJObject() },
            { "state", authorizationResponse.State ?? string.Empty }
        };

        return jObject;
    }
}
