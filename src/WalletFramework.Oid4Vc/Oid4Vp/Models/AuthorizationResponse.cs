using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

/// <summary>
///  Represents an OpenID4VP Authorization Response.
/// </summary>
public class AuthorizationResponse
{
    /// <summary>
    ///     Gets or sets the VP Token.
    /// </summary>
    [JsonProperty ("vp_token")]
    public string VpToken { get; set; } = null!;
        
    /// <summary>
    ///   Gets or sets the Presentation Submission.
    /// </summary>
    [JsonProperty ("presentation_submission")]
    public string PresentationSubmission { get; set; } = null!;
        
    /// <summary>
    ///     Gets or sets the State.
    /// </summary>
    [JsonProperty ("state", NullValueHandling = NullValueHandling.Ignore)]
    public string? State { get; set; } = null!;
}
