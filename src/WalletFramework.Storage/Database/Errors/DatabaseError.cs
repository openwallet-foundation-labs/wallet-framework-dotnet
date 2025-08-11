using WalletFramework.Core.Functional;

namespace WalletFramework.Storage.Database.Errors;

public record DatabaseError(string Reason, Exception? Inner = null) : Error(Reason, Inner);
