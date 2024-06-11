using LanguageExt;

namespace WalletFramework.Functional;

public static class OptionFun
{
    public static Option<T> Some<T>(T value) => value;
    
    public static Option<T> None<T>() => Option<T>.None;
}
