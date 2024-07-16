using LanguageExt;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.Core.Functional;

public abstract record Error(string Message, Option<Exception> Exception)
{
    protected Error(string message) : this(message, Option<Exception>.None)
    {
    }
    
    protected Error(Exception exception) : this(exception.Message, exception)
    {
    }
}

public static class ErrorFun
{
    public static Validation<T> OnError<T, TError>(Exception e) where TError : Error => 
        (TError) Activator.CreateInstance(typeof(TError), e)!;
    
    public static string MessagesJoined(this Seq<Error> errors) =>
        string.Join(", ", errors.Map(e => e.Message));
    
    public static IEnumerable<string> Messages(this Seq<Error> errors) =>
        errors.Map(e => e.Message);

    public static Validation<T> ToInvalid<T>(this Error error) => Invalid<T>(Seq.create(error));

    public static Validation<T> ToInvalid<T>(this Seq<Error> errors) => Invalid<T>(errors);
}
