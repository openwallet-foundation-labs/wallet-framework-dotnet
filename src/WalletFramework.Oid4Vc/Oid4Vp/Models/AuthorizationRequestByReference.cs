using Hyperledger.Aries.Utils;
using LanguageExt;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record AuthorizationRequestByReference
{
    public Uri AuthorizationRequestUri { get; }

    public Uri RequestUri { get; }

    private AuthorizationRequestByReference(Uri authorizationRequestUri, Uri requestUri) => (AuthorizationRequestUri, RequestUri) = (authorizationRequestUri, requestUri);
    
    public static Option<AuthorizationRequestByReference> CreateAuthorizationRequestByReference(Uri uri)
    {
        var clientId = uri.GetQueryParam("client_id");
        
        var requestUri = uri.GetQueryParam("request_uri");
        
        if (string.IsNullOrEmpty(clientId)
            || string.IsNullOrEmpty(requestUri)) 
            return Option<AuthorizationRequestByReference>.None;
        
        return new AuthorizationRequestByReference(uri, new Uri(requestUri));
    }
}
