using System.Web;
using LanguageExt;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record AuthorizationRequestByReference
{
    public Uri AuthorizationRequestUri { get; }

    public Uri RequestUri { get; }
    
    public Option<string> RequestUriMethod { get; }

    private AuthorizationRequestByReference(Uri authorizationRequestUri, Uri requestUri, Option<string> requestUriMethod) => 
        (AuthorizationRequestUri, RequestUri, RequestUriMethod) = (authorizationRequestUri, requestUri, requestUriMethod);
    
    public static Option<AuthorizationRequestByReference> CreateAuthorizationRequestByReference(Uri uri)
    {
        var queryString = HttpUtility.ParseQueryString(uri.Query);
        var clientId = queryString["client_id"];
        var requestUri = queryString["request_uri"];
        var requestUriMethod = queryString["request_uri_method"];
        
        if (string.IsNullOrEmpty(clientId)
            || string.IsNullOrEmpty(requestUri)) 
            return Option<AuthorizationRequestByReference>.None;
        
        return new AuthorizationRequestByReference(uri, new Uri(requestUri), requestUriMethod);
    }
}
