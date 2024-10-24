using Hyperledger.Aries.Utils;
using LanguageExt;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record AuthorizationRequestByValue
{
    public Uri RequestUri { get; }

    public Option<Uri> PresentationDefinitionUri { get; }

    private AuthorizationRequestByValue(Uri requestUri, Option<Uri> presentationDefinitionUri) => (RequestUri, PresentationDefinitionUri) = (requestUri, presentationDefinitionUri);
    
    public static Option<AuthorizationRequestByValue> CreateAuthorizationRequestByValue(Uri uri)
    {
        var clientId = uri.GetQueryParam("client_id");
        var nonce = uri.GetQueryParam("nonce");
        
        var presentationDefinition = uri.GetQueryParam("presentation_definition");
        var presentationDefinitionUri = uri.GetQueryParam("presentation_definition_uri");
        var scope = uri.GetQueryParam("scope");
        
        if (string.IsNullOrEmpty(clientId) 
            || string.IsNullOrEmpty(nonce) 
            || string.IsNullOrEmpty(presentationDefinition) && string.IsNullOrEmpty(presentationDefinitionUri) && string.IsNullOrEmpty(scope)) 
            return Option<AuthorizationRequestByValue>.None;
        
        return new AuthorizationRequestByValue(uri, string.IsNullOrEmpty(presentationDefinitionUri) ? Option<Uri>.None : new Uri(presentationDefinitionUri));
    }
}
