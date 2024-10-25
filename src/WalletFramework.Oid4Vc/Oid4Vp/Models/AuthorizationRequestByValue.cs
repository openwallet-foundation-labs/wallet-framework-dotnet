using System.Web;
using LanguageExt;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record AuthorizationRequestByValue
{
    public Uri RequestUri { get; }

    public Option<Uri> PresentationDefinitionUri { get; }

    private AuthorizationRequestByValue(Uri requestUri, Option<Uri> presentationDefinitionUri) => (RequestUri, PresentationDefinitionUri) = (requestUri, presentationDefinitionUri);
    
    public static Option<AuthorizationRequestByValue> CreateAuthorizationRequestByValue(Uri uri)
    {
        var queryString = HttpUtility.ParseQueryString(uri.Query);
        var clientId = queryString["client_id"];
        var nonce = queryString["nonce"];
        var presentationDefinition = queryString["presentation_definition"];
        var presentationDefinitionUri = queryString["presentation_definition_uri"];
        var scope = queryString["scope"];
        
        if (string.IsNullOrEmpty(clientId) 
            || string.IsNullOrEmpty(nonce) 
            || string.IsNullOrEmpty(presentationDefinition) && string.IsNullOrEmpty(presentationDefinitionUri) && string.IsNullOrEmpty(scope)) 
            return Option<AuthorizationRequestByValue>.None;
        
        return new AuthorizationRequestByValue(uri, string.IsNullOrEmpty(presentationDefinitionUri) ? Option<Uri>.None : new Uri(presentationDefinitionUri));
    }
}
