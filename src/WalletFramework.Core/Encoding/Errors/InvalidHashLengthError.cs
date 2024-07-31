using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Encoding.Errors;

public record InvalidHashLengthError() : Error("The byte string hash cannot be 0");
