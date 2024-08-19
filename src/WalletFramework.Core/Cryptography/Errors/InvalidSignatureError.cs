using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Cryptography.Errors;

public record InvalidSignatureError(string Message, Exception E) : Error(Message, E);
