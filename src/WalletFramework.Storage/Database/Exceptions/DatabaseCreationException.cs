namespace WalletFramework.Storage.Database.Exceptions;

public sealed class DatabaseCreationException : Exception
{
    public DatabaseCreationException(string message) : base(message) { }

    public DatabaseCreationException(string message, Exception innerException)
        : base(message, innerException) { }
}
