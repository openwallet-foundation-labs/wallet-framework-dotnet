using OneOf;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public struct AuthorizationRequestUri
{
    public OneOf<AuthorizationRequestByReference, AuthorizationRequestByValue> Value { get; }
        
    private AuthorizationRequestUri(OneOf<AuthorizationRequestByReference, AuthorizationRequestByValue> value) => Value = value;
    
    public static AuthorizationRequestUri FromUri(Uri uri)
    {
        return AuthorizationRequestByReference.CreateAuthorizationRequestByReference(uri).Match(
            byReference => new AuthorizationRequestUri(byReference),
            () => AuthorizationRequestByValue.CreateAuthorizationRequestByValue(uri).Match(
                byValue => new AuthorizationRequestUri(byValue),
                () => throw new InvalidOperationException("Authorization Request Uri is not a valid")
            )
        );
    }
}
