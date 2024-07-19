using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Error;

/// <summary>
///     Represents an error response when the request is invalid or unauthorized.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    ///     Gets or sets the error code indicating the type of error that occurred.
    /// </summary>
    [JsonProperty("error")]
    public string Error { get; set; }

    /// <summary>
    ///     Gets or sets the human-readable text providing additional information about the error.
    /// </summary>
    [JsonProperty("error_description")]
    public string ErrorDescription { get; set; }

    /// <summary>
    ///     Gets or sets the URI identifying a human-readable web page with information about the error.
    /// </summary>
    [JsonProperty("error_uri")]
    public string ErrorUri { get; set; }
}