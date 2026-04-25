using System.Web;
using LanguageExt;

namespace WalletFramework.Oid4Vp.Models;

public record AuthorizationRequestByValue
{
    public Uri RequestUri { get; }

    private AuthorizationRequestByValue(Uri requestUri) => RequestUri = requestUri;
    
    public static Option<AuthorizationRequestByValue> CreateAuthorizationRequestByValue(Uri uri)
    {
        var queryString = HttpUtility.ParseQueryString(uri.Query);
        var clientId = queryString["client_id"];
        var nonce = queryString["nonce"];
        var dcqlQuery = queryString["dcql_query"];
        var scope = queryString["scope"];
        
        if (string.IsNullOrEmpty(clientId) 
            || string.IsNullOrEmpty(nonce) 
            || string.IsNullOrEmpty(dcqlQuery) && string.IsNullOrEmpty(scope)) 
            return Option<AuthorizationRequestByValue>.None;
        
        return new AuthorizationRequestByValue(uri);
    }
}
