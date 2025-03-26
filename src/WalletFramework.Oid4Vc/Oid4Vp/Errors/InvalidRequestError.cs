using LanguageExt;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vp.Errors;

public record InvalidRequestError : VpError
{
    private const string Code = "invalid_request";
    
    public Option<Error> WrappedError { get; } = Option<Error>.None;

    public InvalidRequestError(string message) : base(Code, message)
    {
    }
    
    public InvalidRequestError(string message, Error wrappedError) : base(Code, message)
    {
        WrappedError = wrappedError;
    }

    public InvalidRequestError(string Message, Exception Exception) 
        : base(Code, Message, Exception)
    {
    }
}

public static class InvalidRequestErrorFun
{
    public static Error ToError(this InvalidRequestError invalidRequestError) =>
        invalidRequestError.WrappedError.Match(
            wrappedError => wrappedError,
            () => invalidRequestError);
}
