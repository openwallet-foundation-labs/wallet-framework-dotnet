namespace WalletFramework.Oid4Vp.Errors;

public record OriginMismatchError : InvalidRequestError
{
    public OriginMismatchError(string message) : base(message)
    {
    }

    public OriginMismatchError(string Message, Exception Exception) 
        : base(Message, Exception)
    {
    }
}

