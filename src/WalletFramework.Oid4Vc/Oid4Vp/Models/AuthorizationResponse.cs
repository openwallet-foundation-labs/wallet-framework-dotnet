using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;

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
    public VpToken VpToken { get; set; } = null!;
        
    /// <summary>
    ///   Gets or sets the Presentation Submission.
    /// </summary>
    [JsonProperty("presentation_submission", NullValueHandling = NullValueHandling.Ignore)]
    public PresentationSubmission? PresentationSubmission { get; set; }
        
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
            { "vp_token", authorizationResponse.VpToken.AsString() },
            { "state", authorizationResponse.State ?? string.Empty }
        };
        
        if(authorizationResponse.PresentationSubmission is not null)
            dict.Add("presentation_submission", JsonConvert.SerializeObject(authorizationResponse.PresentationSubmission));

        return new FormUrlEncodedContent(dict);
    }
}
