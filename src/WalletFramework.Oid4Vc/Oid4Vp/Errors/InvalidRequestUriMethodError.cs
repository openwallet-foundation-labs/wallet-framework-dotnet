namespace WalletFramework.Oid4Vc.Oid4Vp.Errors;

/// <summary>
/// Error indicating that the request_uri_method is not supported or invalid.
/// This error is defined in OpenID4VP specification section 5.10.2.
/// </summary>
public record InvalidRequestUriMethodError : VpError
{
    private const string Code = "invalid_request_uri_method";
    
    public InvalidRequestUriMethodError(string message) : base(Code, message) { }
} 
