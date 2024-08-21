using Hyperledger.Aries.Utils;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

/// <summary>
///    This class represents a haip conform OpenID4VP Authorization Request Uri.
/// </summary>
public class HaipAuthorizationRequestUri
{
    /// <summary>
    ///     Gets or sets the uri of the request.
    /// </summary>
    public Uri Uri { get; set; } = null!;
        
    /// <summary>
    ///     Gets or sets the value of the request_uri parameter.
    /// </summary>
    public string RequestUri { get; set; } = null!;
        
    /// <summary>
    ///    Validates the hap conformity of an uri and returns a HaipAuthorizationRequestUri.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns>The HaipAuthorizationRequestUri</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static HaipAuthorizationRequestUri FromUri(Uri uri)
    {
        var request = uri.GetQueryParam("request_uri");
        if (string.IsNullOrEmpty(request))
            throw new InvalidOperationException("HAIP requires request_uri parameter");

        return new HaipAuthorizationRequestUri()
        {
            RequestUri = request,
            Uri = uri
        };
    }
}
