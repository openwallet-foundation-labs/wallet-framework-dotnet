using System.Net;

namespace WalletFramework.Oid4Vc.Oid4Vci.Exceptions;

/// <summary>
///     Represents an exception thrown when the grant within the Oid4Vci flow is invalid (e.g. wrong tx_code).
/// </summary>
public class Oid4VciInvalidGrantException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Oid4VciInvalidGrantException"/> class.
    /// </summary>
    /// <param name="statusCode">The StatusCode associated with the thrown Exception</param>
    public Oid4VciInvalidGrantException(HttpStatusCode statusCode)
        : base($"Invalid grant error. Status Code is {statusCode}")
    {
    }
}